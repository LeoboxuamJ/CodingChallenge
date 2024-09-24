using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetCoding.Core.Models;

namespace DotnetCoding.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDetails>> GetAllProducts();
        Task<ProductDetails> GetProductById(int id);
        Task AddProduct(ProductDetails product);
        Task UpdateProduct(int id, ProductDetails updatedProduct);
    }

    public interface IApprovalQueueService
    {
        // Adds a product to the approval queue with a reason for the request
        Task PushToQueue(ProductDetails product, string reason);

        // Retrieves all products currently in the approval queue
        Task<IEnumerable<ApprovalQueueItem>> GetQueue();

        // Retrieves a specific approval queue request by its ID
        Task<ApprovalQueueItem> GetRequestById(int id);

        // Approves a product request and updates its state in the system
        Task ApproveRequest(int id);

        // Rejects a product request, leaving the product in its original state
        Task RejectRequest(int id);
    }
}
