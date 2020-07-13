﻿using System;
using System.Threading.Tasks;
using Angy.Model;
using Angy.Shared.Gateways;
using Angy.Shared.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Angy.BackEndClient.Pages.ProductsPage
{
    public class ProductDetailComponent : ComponentBase
    {
        [Parameter] public Guid ProductId { get; set; }

        [Inject] public ProductGateway ProductGateway { get; set; } = null!;
        [Inject] public MicroCategoryGateway MicroCategoriesGateway { get; set; } = null!;
        [Inject] public NavigationManager NavigationManager { get; set; } = null!;

        protected EditContext EditContext = new EditContext(new ProductViewModel());
        protected ProductViewModel ViewModel { get; private set; } = new ProductViewModel();

        protected override async Task OnInitializedAsync()
        {
            if (ProductId == Guid.Empty)
            {
                var result = await Result.Try(MicroCategoriesGateway.GetMicroCategoriesWithIdAndName);

                ViewModel = new ProductViewModel(result.Success);
            }
            else
            {
                var result = await Result.Try(() => ProductGateway.GetProductByIdWithMicroCategories(ProductId));
                var (product, micros) = result.Success;

                ViewModel = new ProductViewModel(product, micros);
            }

            EditContext = new EditContext(ViewModel);
        }

        protected async Task HandleSubmit()
        {
            if (!EditContext.Validate()) return;

            if (ViewModel.Product.Id == Guid.Empty)
                await Result.Try(() => ProductGateway.CreateProduct(ViewModel.Product));
            else
                await Result.Try(() => ProductGateway.UpdateProduct(ProductId, ViewModel.Product));

            NavigationManager.NavigateTo("products");
        }
    }
}