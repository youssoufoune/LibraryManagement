using LibraryData.Models;
using System;
using System.Collections.Generic;

namespace LibraryData
{
    public interface ICheckout
    {   
        // Checkouts management
        IEnumerable<Checkout> getAll();
        IEnumerable<CheckoutHistory> GetCheckoutHistory(int id);

        Checkout GetById(int checkoutId);
        Checkout GetLatestCheckout(int assetId);

        string GetCurrentCheckoutPatron(int assetId);

        void Add(Checkout newCheckout);
        void CheckOutItem(int assetId, int libraryCardId);
        void CheckInItem(int assetId);
        bool IsCheckedOut(int id);
        string GetCurrentPatron(int id);
        


        // Holds management
        DateTime GetCurrentHoldPlaced(int id);

        IEnumerable<Hold> GetCurrentHolds(int id);

        string GetCurrentHoldPatronName(int id);

        void PlaceHold(int assetId, int libraryCardId);
        
        
        // Lost and found management
        void MarkLost(int assetId);
        void MarkFound(int assetId);
        
    }
}
