using StockWise.Client.Models;
using StockWise.Client.PageModels;

namespace StockWise.Client.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}