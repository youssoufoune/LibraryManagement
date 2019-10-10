using LibraryData;
using LibraryData.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibraryServices
{
    public class CheckoutService : ICheckout
    {
        private LibraryContext _context;

        public CheckoutService(LibraryContext context)
        {
            _context = context;
        }


        public void Add(Checkout newCheckout)
        {
            _context.Add(newCheckout);
            _context.SaveChanges();
        }



        public void CheckInItem(int assetId) 
        {
            var now = DateTime.Now;
            var item = _context.LibraryAssets
                .FirstOrDefault(a => a.Id == assetId);

            RemoveExistingCheckouts(assetId); // remove any existing checkouts on the item

            CloseExistingCheckoutHistory(assetId, now); // Close any existing checkout history

            var currentholds = _context.Holds       // Look for existing holds on the item
                .Include(h=>h.LibraryAsset)
                .Include(h=>h.LibraryCard)
                .Where(h => h.LibraryAsset.Id  == assetId);

            if (currentholds.Any())         // if there are holds checkout the item to the librarycard with the earliest hold
            {
                CheckoutToEarliestHold(assetId, currentholds);
                return;
            }

            UpdateAssetStatus(assetId, "Availabe");
            _context.SaveChanges();
        }



        private void CheckoutToEarliestHold(int assetId, IQueryable<Hold> currentholds)
        {
            var earliestHold = currentholds
                .OrderBy(h =>h.HoldPlaced)
                .FirstOrDefault();

            var card = earliestHold.LibraryCard;
            _context.Remove(earliestHold);
            _context.SaveChanges();
            CheckOutItem(assetId, card.Id);
        }


        /*----------------------------------------------Add to checkout table------------------------------------------------------------*/
        public void CheckOutItem(int assetId, int libraryCardId)
        {
            if (IsCheckedOut(assetId))
            {
                return; // logic to handle feedback
            }

            var item = _context.LibraryAssets
                .FirstOrDefault(a => a.Id == assetId);

            UpdateAssetStatus(assetId, "Checked Out");

            var libraryCard = _context.LibraryCards
                .Include(card => card.Checkouts)
                .FirstOrDefault(card => card.Id == libraryCardId);

            var now = DateTime.Now;
            var checkout = new Checkout
            {
                LibraryAsset = item,
                LibraryCard = libraryCard,
                Since= now,
                Until = GetDefaultCheckoutTime(now)
            };

            _context.Add(checkout);

            var checkoutHistory = new CheckoutHistory
            {
                CheckedOut = now,
                LibraryAsset=item,
                LibraryCard= libraryCard
            };
            _context.Add(checkoutHistory);
            _context.SaveChanges();
        }



        private DateTime GetDefaultCheckoutTime(DateTime now)
        {
            return now.AddDays(30);
        } // how long a particular item can be checked out
        


        public bool IsCheckedOut(int assetId)
        {
            return _context.Checkouts
                .Where(c => c.LibraryAsset.Id == assetId).Any();
        }
        /*-----------------------------------------------------------------------------------------------------------*/


        public IEnumerable<Checkout> getAll()
        {
           return _context.Checkouts;
        }



        public Checkout GetById(int checkoutId)
        {
            return getAll()
                .FirstOrDefault(checkout => checkout.Id == checkoutId);
        }



        public IEnumerable<CheckoutHistory> GetCheckoutHistory(int id)
        {
            return _context.CheckoutHistories
                 .Include(h => h.LibraryAsset)
                 .Include(h => h.LibraryCard)
                .Where(h=> h.LibraryAsset.Id == id);
        }


        public string GetCurrentPatron(int id) {

            var checkout = _context.Checkouts
                .Include(a => a.LibraryAsset)
                .Include(a => a.LibraryCard)
                .FirstOrDefault(a => a.LibraryAsset.Id == id);

            if (checkout == null) return "Not checked out.";

            var cardId = checkout.LibraryCard.Id;

            var patron = _context.Patrons
                    .Include(p => p.LibraryCard)
                    .FirstOrDefault(p => p.LibraryCard.Id == cardId);
            return patron?.FirstName + " " + patron?.LastName;
        }


        public string GetCurrentHoldPatronName(int holdId)
        {
            var hold = _context.Holds
                .Include(h => h.LibraryAsset)
                .Include(h => h.LibraryCard)
                .FirstOrDefault(h => h.Id == holdId);

            var cardId = hold?.LibraryCard.Id; // if hold is null cardId is null

            var patron = _context.Patrons
                .Include(p => p.LibraryCard)
                .FirstOrDefault(p => p.LibraryCard.Id == cardId);

            return patron?.FirstName + " " + patron?.LastName; // if card id is null patron is null

        }



        public DateTime GetCurrentHoldPlaced(int holdId)
        {
            return _context.Holds
                .Include(h => h.LibraryAsset)
                .Include(h => h.LibraryCard)
                .FirstOrDefault(h => h.Id == holdId)
                .HoldPlaced;              
        }



        public IEnumerable<Hold> GetCurrentHolds(int id)
        {
            return _context.Holds
                .Include(h=>h.LibraryAsset)
                .Where(h=>h.LibraryAsset.Id == id);
        }



        public Checkout GetLatestCheckout(int assetId)
        {
            return _context.Checkouts
                .Where(Checkout => Checkout.LibraryAsset.Id == assetId)
                .OrderByDescending(Checkout=> Checkout.Since)
                .FirstOrDefault();
        }



        public void MarkFound(int assetId)
        {

            var now = DateTime.Now;
            //var item = _context.LibraryAssets
            //    .FirstOrDefault(a => a.Id == assetId);


            //_context.Update(item);
            //item.Status = _context.Statuses
            //    .FirstOrDefault(status => status.Name == "Available");
            UpdateAssetStatus(assetId,"Available");

            
            // remove any existing checkouts on this item
            //var checkout = _context.Checkouts
            //    .FirstOrDefault(c => c.LibraryAsset.Id == assetId);

            //if (checkout != null)
            //{
            //    _context.Remove(checkout);
            //}
            RemoveExistingCheckouts(assetId);

            // close existing checkout history
            //var history = _context.CheckoutHistories                                                                                                                                                                  
            //    .FirstOrDefault(h => h.LibraryAsset.Id == assetId 
            //    && h.CheckedIn == null);

            //if (history != null)
            //{
            //    _context.Update(history);
            //    history.CheckedIn = now;
            //}
            CloseExistingCheckoutHistory(assetId, now);

            _context.SaveChanges();
        }


        // refactoring update status
        private void UpdateAssetStatus(int assetId, string newStatus)
        {
            var item = _context.LibraryAssets
               .FirstOrDefault(a => a.Id == assetId);

            _context.Update(item);

            item.Status = _context.Statuses
                .FirstOrDefault(status => status.Name ==  newStatus);


        }



        // Refactoring close existing checkout history
        private void CloseExistingCheckoutHistory(int assetId, DateTime now)
        {
            var history = _context.CheckoutHistories
                .FirstOrDefault(h => h.LibraryAsset.Id == assetId
                && h.CheckedIn == null);

            if (history != null)
            {
                _context.Update(history);
                history.CheckedIn = now;
            }
        }


        // Refactoring remove any existing checkouts on this item
        private void RemoveExistingCheckouts(int assetId)
        {
            var checkout = _context.Checkouts
                 .FirstOrDefault(c => c.LibraryAsset.Id == assetId);

            if (checkout != null)
            {
                _context.Remove(checkout);
            }
        }


        

        public void MarkLost(int assetId)
        {
            UpdateAssetStatus(assetId, "Lost");
            _context.SaveChanges();
        }



        public void PlaceHold(int assetId, int libraryCardId)
        {
            var now = DateTime.Now;
            var asset = _context.LibraryAssets
                .Include(a => a.Status)
                .FirstOrDefault(a => a.Id == assetId);

            var card = _context.LibraryCards
                .FirstOrDefault(c => c.Id == libraryCardId);

            if (asset.Status.Name == "Available")
            {
                UpdateAssetStatus(assetId, "On Hold");
            }

            var hold = new Hold
            {
                HoldPlaced= now,
                LibraryAsset = asset,
                LibraryCard = card
            };
            _context.Add(hold);
            _context.SaveChanges();
        }

        public string GetCurrentCheckoutPatron(int assetId)
        {
            var checkout = GetCheckoutByAssetId(assetId);
            if (checkout == null)
            {
                return "Not Checked Out";
            };
            var cardId = checkout.LibraryCard.Id;
            var patron = _context.Patrons
                .Include(p => p.LibraryCard)
                .FirstOrDefault(p => p.LibraryCard.Id == cardId);

            return patron.FirstName + " " + patron.LastName; 

        }

        private Checkout GetCheckoutByAssetId(int assetId)
        {
            return _context.Checkouts
                .Include(co => co.LibraryAsset)
                .Include(co=>co.LibraryCard)
                .FirstOrDefault(co=>co.LibraryAsset.Id == assetId);
        }

        
    }
}
