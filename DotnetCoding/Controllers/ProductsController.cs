using Microsoft.AspNetCore.Mvc;
using DotnetCoding.Core.Models;
using DotnetCoding.Services.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetCoding.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IApprovalQueueService _approvalQueueService;

        public ProductsController(IProductService productService, IApprovalQueueService approvalQueueService)
        {
            _productService = productService;
            _approvalQueueService = approvalQueueService;
        }

        /// <summary>
        /// Get the list of product
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetProductList()
        {
            var productDetailsList = await _productService.GetAllProducts();
            if(productDetailsList == null)
            {
                return NotFound();
            }
            return Ok(productDetailsList);
        }       

        /// Create a new product 
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] ProductDetails product)
        {
            if (product.ProductPrice > 10000)
            {
                return BadRequest("Product price exceeds the maximum limit of $10,000.");
            }

            if (product.ProductPrice > 5000)
            {
                await _approvalQueueService.PushToQueue(product, "Creation: Price exceeds $5000.");
            }

            await _productService.AddProduct(product);
            return Ok(product);
        }

        /// Update an existing product with business rules
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductDetails updatedProduct)
        {
            var existingProduct = await _productService.GetProductById(id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            // Check if price increase is more than 50%
            if (updatedProduct.ProductPrice > existingProduct.ProductPrice * 1.5M || updatedProduct.ProductPrice > 5000)
            {
                await _approvalQueueService.PushToQueue(updatedProduct, "Update: Price change exceeds limit.");
            }

            await _productService.UpdateProduct(id, updatedProduct);
            return Ok(updatedProduct);
        }

        /// Delete a product by sending it to the approval queue
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _productService.GetProductById(id);
            if (product == null)
            {
                return NotFound();
            }

            await _approvalQueueService.PushToQueue(product, "Delete: Product marked for deletion.");
            return Ok("Product deletion request sent to approval queue.");
        }

        /// Get all products from the approval queue
        [HttpGet("queue")]
        public async Task<IActionResult> GetApprovalQueue()
        {
            var queueItems = await _approvalQueueService.GetQueue();
            if (queueItems == null)
            {
                return NotFound();
            }
            return Ok(queueItems.OrderBy(q => q.RequestDate).ToList());
        }

        /// Approve a product request from the approval queue
        [HttpPost("queue/{id}/approve")]
        public async Task<IActionResult> ApproveProductInQueue(int id)
        {
            var queueItem = await _approvalQueueService.GetRequestById(id);
            if (queueItem == null)
            {
                return NotFound();
            }

            await _approvalQueueService.ApproveRequest(id);
            return Ok("Product approved and updated in the system.");
        }

        /// Reject a product request from the approval queue
        [HttpPost("queue/{id}/reject")]
        public async Task<IActionResult> RejectProductInQueue(int id)
        {
            var queueItem = await _approvalQueueService.GetRequestById(id);
            if (queueItem == null)
            {
                return NotFound();
            }

            await _approvalQueueService.RejectRequest(id);
            return Ok("Product request rejected.");
        }
    }
}
