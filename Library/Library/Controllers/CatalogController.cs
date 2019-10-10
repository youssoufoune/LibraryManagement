using Library.Models.Catalog;
using Library.Models.CheckoutModels;
using LibraryData;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace Library.Controllers
{
    public class CatalogController : Controller
    {
        private ILibraryAsset _assets;
        private ICheckout _checkouts;

        public CatalogController(ILibraryAsset assets, ICheckout checkouts) // inject _asset and _checkout into the constructor
        {
            _assets = assets;
            _checkouts = checkouts;
        }

        public IActionResult Index()
        {
            var assetModels = _assets.GetAll();
            var listingResult = assetModels.Select(result => new AssetIndexListingModel // AssetModels is getting all the data coming from db then  
            {                                                                           // project all the elements of assetModels results into a new form
                Id = result.Id,                                                         // so any type of other object that we might want to map it to in this case we map it to AssetIndexListingModel
                ImageUrl = result.ImageUrl,
                AuthorOrDirector = _assets.GetAuthorOrDirector(result.Id),
                DeweyCallNumber = _assets.GetDeweyIndex(result.Id),
                Title = result.Title,
                Type = _assets.GetType(result.Id)
            });

            var model = new AssetIndexModel()
            { // pass new instance of AssetIndexModel
                Assets = listingResult          // pass listing results we just created to the property of the model.
            };

            return View(model);


        }

        public IActionResult Details(int id)
        {
            var asset = _assets.GetById(id);// retrieve an asset from the db represented in the form of domain model
            var currentHolds = _checkouts.GetCurrentHolds(id)
                .Select(a => new AssetHoldModel
                {
                    HoldPlaced=_checkouts.GetCurrentHoldPlaced(a.Id).ToString("d"),
                    PatronName=_checkouts.GetCurrentHoldPatronName(a.Id)

                });
            var model = new AssetDetailModel    // pass it to the view in the form of the view model that we just created
            {
                AssetId = id,
                Title = asset.Title,
                Type=_assets.GetType(id),
                Year = asset.Year,
                Cost = asset.Cost,
                Status = asset.Status.Name,
                ImageUrl = asset.ImageUrl,
                AuthotOrDirector = _assets.GetAuthorOrDirector(id),
                CurrentLocation = _assets.GetCurrentLocation(id).Name,
                DeweyCallNumber = _assets.GetDeweyIndex(id),
                ISBN = _assets.GetIsbn(id),
                LatestCheckout = _checkouts.GetLatestCheckout(id),
                CheckoutHistory = _checkouts.GetCheckoutHistory(id),
                PatronName = _checkouts.GetCurrentPatron(id),
                CurrentHolds= currentHolds
            };

            return View(model);

        }

        public IActionResult Checkout(int id)
        {
            var asset = _assets.GetById(id);
            var model = new CheckoutModel
            {
                AssetId = id,
                ImageUrl = asset.ImageUrl,
                Title= asset.Title,
                LibraryCardId= "",
                IsCheckedOut = _checkouts.IsCheckedOut(id)
            };
            return View(model);
        }

        public IActionResult CheckIn(int id) 
        {
            _checkouts.CheckInItem(id);
            return RedirectToAction("Details", new { id = id });
        }


        public IActionResult Hold(int id)
        {
            var asset = _assets.GetById(id);
            var model = new CheckoutModel
            {
                AssetId = id,
                ImageUrl = asset.ImageUrl,
                Title = asset.Title,
                LibraryCardId = "",
                IsCheckedOut = _checkouts.IsCheckedOut(id),
                HoldCount = _checkouts.GetCurrentHolds(id).Count()
            };
            return View(model);

        }


        public IActionResult MarkLost(int assetId)
        {
            _checkouts.MarkLost(assetId);
            return RedirectToAction("Details", new { id = assetId });
        }


        public IActionResult MarkFound(int assetId)
        {
            _checkouts.MarkFound(assetId);
            return RedirectToAction("Details", new { id = assetId });
        }

        [HttpPost] // the following action only support the httppost method
        public IActionResult PlaceCheckout(int assetId, int libraryCardId)
        {
            _checkouts.CheckOutItem(assetId, libraryCardId);
            return RedirectToAction("Details",new { id = assetId});
        }

        [HttpPost] // the following action only support the httppost method
        public IActionResult PlaceHold(int assetId, int libraryCardId)
        {
            _checkouts.PlaceHold(assetId, libraryCardId);
            return RedirectToAction("Details", new { id = assetId });
        }
        
    }
}
