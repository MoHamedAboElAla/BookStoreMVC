using Ecommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Ecommerce.DataAcess.Data;
using Ecommerce.DataAcess.Repository.IRepository;
using Ecommerce.Models.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ecommerce.Models.ViewModels;
using Microsoft.AspNetCore.Http.Metadata;
using Ecommerce.Utility;
using Microsoft.AspNetCore.Authorization;


namespace EcommerceProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
     
        }
        public IActionResult Index()
        {
            var CompanyList = _unitOfWork.Company.GetAll();
            
            return View("Index", CompanyList);
        }
        public IActionResult Upsert(int? id)
        {
           
            if (id == null || id == 0)
            {
                return View(new Company());
            }
            else
            {
                Company company = _unitOfWork.Company.Get(u => u.Id==id);
                return View(company);
            }
        }
        [HttpPost]
        public IActionResult Upsert(Company company)
        {
        

            if (ModelState.IsValid)
            {

                    if (company.Id == 0)
                    {
                        _unitOfWork.Company.Add(company);
                    }
                    else
                    {
                        _unitOfWork.Company.Update(company);
                    }

                    _unitOfWork.Save();
                    TempData["success"] = "Product created successfully";
                    return RedirectToAction("Index");
                }

                else
                {

                   

                    return View(company);
                }

            }
        
      /*  public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var product = _unitOfWork.Product.Get(i => i.Id == id);
            if (product == null)
                return NotFound();


            return View(product);
        }
        [HttpPost]
        public IActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Product.Update(product);
                _unitOfWork.Save();
                TempData["success"] = "Product updated successfully";
                return RedirectToAction("Index");

            }
            return View();

        }*/

      /*        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var product = _unitOfWork.Product.Get(i => i.Id == id);
            if (product == null)
                return NotFound();


            return View(product);
        }
*/        
        
     /*   [HttpPost, ActionName("Delete")]
        public IActionResult DeleteProduct(int? id)
        {
            var product = _unitOfWork.Product.Get(i => i.Id == id);
            if (product == null)
            {
                return NotFound();

            }
            _unitOfWork.Product.Remove(product);
            _unitOfWork.Save();
            TempData["success"] = "Product deleted successfully";
            return RedirectToAction("Index");

        }*/
        #region API CALLS
   
        
        [HttpGet]
        public IActionResult GetAll() {
            var CompanyList = _unitOfWork.Company.GetAll();
            return Json(new { data = CompanyList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var CompanytoDeleted = _unitOfWork.Company.Get(i => i.Id == id);
            if (CompanytoDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
           
           _unitOfWork.Company.Remove(CompanytoDeleted);
           _unitOfWork.Save();
           return Json(new { success = true, message = "Delete successful" });
        }

        #endregion
    }
}
