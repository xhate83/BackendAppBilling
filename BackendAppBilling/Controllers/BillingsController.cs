using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendAppBilling.Models;
using Microsoft.OpenApi.Any;
using Azure;

namespace BackendAppBilling.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BillingsController : ControllerBase
    {
        private readonly DbbillingContext _context;

        public BillingsController(DbbillingContext context)
        {
            _context = context;
        }

        // GET: api/Billings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AnyType>>> GetBillings()
        {
            var billingsWithDetails =
                (from billing in _context.Billings 
                     join detail in _context.Details on billing.Id equals detail.IdBilling
                     join customer in _context.Customers on billing.IdCustomer equals customer.Id
                     join product in _context.Products on detail.IdProduct equals product.Id
                 group new { billing, detail, customer, product } by new { billing.Id, billing.IdCustomer } into global
                 select new
                {
                    global.Key.Id,
                    createdAt = global.Max(x => x.billing.CreatedAt),
                    idCustomer = global.Key.IdCustomer,
                    customer = new
                    {
                        id = global.Key.IdCustomer,
                        name = global.Max(x => x.customer.Name),
                        age = global.Max(x => x.customer.Age)
                    },
                     products = global.Select(x => new { x.product.Id, x.product.Name, x.detail.Quantity, totalPrice = x.detail.Quantity * x.product.Price, x.product.Price }),
                     totalPrice = global.Sum(x => x.detail.Quantity * x.product.Price),
                 }).ToList();

            return Ok(billingsWithDetails);
        }

        // GET: api/Billings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Billing>> GetBilling(int id)
        {

            var billingWithDetails = (from billing in _context.Billings
                where billing.Id == id
                join detail in _context.Details on billing.Id equals detail.IdBilling
                join product in _context.Products on detail.IdProduct equals product.Id
                join customer in _context.Customers on billing.IdCustomer equals customer.Id
                select new
                {
                    id = billing.Id,
                    customerId = billing.IdCustomer,
                    customerName = customer.Name,
                    customerAge = customer.Age,
                    createdAt = billing.CreatedAt,
                    totalPrice = detail.Quantity * product.Price,
                    Product = new { product.Id, product.Name, detail.Quantity, totalPrice = detail.Quantity * product.Price, product.Price }
                }).ToList()
                .GroupBy(x => new { x.id, x.customerId, x.customerName, x.customerAge, x.createdAt })
                .Select(x => new
                {
                    x.Key.id,
                    idCustomer = x.Key.customerId,
                    customer = new
                    {
                        id = x.Key.customerId,
                        name = x.Key.customerName,
                        age = x.Key.customerAge,
                    },
                    x.Key.createdAt,
                    totalPrice = x.Sum(p => p.Product.totalPrice),
                    products = x.Select(p => p.Product)
                }).SingleOrDefault();

            if (billingWithDetails == null)
            {
                return NotFound();
            }

            return Ok(billingWithDetails);
        }

        // PUT: api/Billings/5
        // Only supports changing customerId
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}/{idNewCustomer}")]
        public async Task<IActionResult> PutBilling(int id, int idNewCustomer)
        {
            var newCustomer = _context.Billings.Find(idNewCustomer);

            if(newCustomer == null)
            {
                return NoContent();
            }

            var billingToUpdate = _context.Billings.Find(id);


            if (billingToUpdate != null)
            {

                if (newCustomer.Id == billingToUpdate.IdCustomer)
                {
                    return BadRequest();
                }

                billingToUpdate.IdCustomer = newCustomer.Id;
            } else
            {
                return NoContent();
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BillingExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            var response = new
            {
                message = "Factura editada correctamente",
                idBilling = billingToUpdate.Id,
            };
            return Ok(response);
        }

        // POST: api/Billings
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<BillingCreate>> PostBilling(BillingCreate newBilling)
        {

            var billingToCreate = new Billing
            {
                CreatedAt = DateTime.Now,
                IdCustomer = newBilling.idCustomer,
            };
             _context.Billings.Add(billingToCreate);
            await _context.SaveChangesAsync();


            List<Detail> details = new();
            foreach (var item in newBilling.idsProducts)
            {
                var product = _context.Products.Find(item.id);
                if (product != null)
                {
                    if ((product.Stock - item.quantity) < 0)
                    {
                        item.quantity = product.Stock;
                        product.Stock = 0; 
                    }
                    else
                    {
                        product.Stock -= item.quantity;
                    }

                }
                var detail = new Detail
                {
                    IdBilling = billingToCreate.Id,
                    IdProduct = item.id,
                    Quantity = item.quantity
                };
                details.Add(detail);
   
            }
            _context.Details.AddRange(details);
            await _context.SaveChangesAsync();

            var response = new
            {
                message = "Factura creada correctamente",
                idBilling = billingToCreate.Id,
            };
            return Ok(response);
        }

        // DELETE: api/Billings/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBilling(int id)
        {
            var billing = _context.Billings.Find(id);
            if (billing == null)
            {
                return NotFound();
            }
       
            var details = _context.Details.Where(d => d.IdBilling == id).ToList();
            foreach (var detail in details)
            {
                var product = _context.Products.Find(detail.IdProduct);
                if(product != null)
                {
                    product.Stock += detail.Quantity;
                    _context.Details.Remove(detail);
                }
                
            }
            // billing.DeletedAt= DateTime.Now;
            _context.Billings.Remove(billing);
            _context.SaveChanges();

            await _context.SaveChangesAsync();
            var response = new
            {
                message = "Factura eliminada correctamente",
            };
            return Ok(response);
        }

        private bool BillingExists(int id)
        {
            return _context.Billings.Any(e => e.Id == id);
        }
    }
}
