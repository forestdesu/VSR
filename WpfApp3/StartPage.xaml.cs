using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp3
{
    /// <summary>
    /// Логика взаимодействия для StartPage.xaml
    /// </summary>
    public partial class StartPage : Page
    {

        bool isFlipped = false;
        int page = 1;
        int maxPage = 1;
        public StartPage()
        {
            InitializeComponent();


            maxPage = CntPage();

            LoadTypes();
            UpdatePage();
        }

        public int CntPage()
        {
            using (DBEntities context = new DBEntities())
            {
                return (int)Math.Ceiling((decimal)context.Agent.ToList().Count() / 5);
            }
        }

        public void LoadTypes()
        {
            using (DBEntities context = new DBEntities())
            {
                var allTypes = context.AgentType.ToList();
                allTypes.Insert(0, new AgentType { Title = "Все" });
                TypeFilterCB.ItemsSource = allTypes;


                var sort = new List<string> { "По наименованию", "По размеру скидки", "По приоритету" };
                SortCB.ItemsSource = sort;
            }
        }

        public int Discount(Agent current_agent)
        {
            using (DBEntities context = new DBEntities())
            {
                var salesWithProducts = context.ProductSale
                    .Where(p => p.AgentID == current_agent.ID)
                    .Join(context.Product, ps => ps.ProductID, p => p.ID, (ps, p) => new { Sale = ps, Product = p })
                    .ToList();

                int sum_prd = salesWithProducts.Sum(sp => (int)sp.Product.MinCostForAgent * sp.Sale.ProductCount);

                int disc = 0;
                switch (sum_prd)
                {
                    case int n when n >= 500000:
                        disc = 25;
                        break;
                    case int n when n >= 150000:
                        disc = 20;
                        break;
                    case int n when n >= 50000:
                        disc = 10;
                        break;
                    case int n when n >= 10000:
                        disc = 5;
                        break;
                }

                return disc;
            }
        }

        public void UpdatePage()
        {
            using (DBEntities context = new DBEntities())
            {
                var currentAgents = context.Agent.ToList();

                if (TypeFilterCB.SelectedIndex > 0)
                {
                    currentAgents = currentAgents.Where(p => p.AgentTypeID.Equals(TypeFilterCB.SelectedIndex)).ToList();
                }

                if (SearchBar.Text.Length > 0)
                {
                    currentAgents = currentAgents.Where(p =>
                        p.Title.ToString().ToLower().Contains(SearchBar.Text.ToString().ToLower()) ||
                        p.Phone.ToString().ToLower().Contains(SearchBar.Text.ToString().ToLower()) ||
                        p.Email.ToString().ToLower().Contains(SearchBar.Text.ToString().ToLower())
                    ).ToList();
                }

                var currentAgents1 = currentAgents.Select(p => new
                {
                    ID = p.ID,
                    Title = p.Title,
                    AgentTypeID = p.AgentType.Title,
                    Address = p.Address,
                    INN = p.INN,
                    KPP = p.KPP,
                    DirectorName = p.DirectorName,
                    Phone = p.Phone,
                    Email = p.Email,
                    DisplayedImagePath = p.Logo == null ? "/images/picture.png" : p.Logo,
                    Priority = p.Priority,
                    Discount = Discount(p),
                    YeralySales = context.ProductSale.ToList().Where(d => d.AgentID.Equals(p.ID) && d.SaleDate.Year == 2019).Count()
                });


                if (SortCB.SelectedIndex == 0)
                {
                    if (isFlipped)
                        LViewAgents.ItemsSource = currentAgents1.OrderByDescending(p => p.Title).ToList().Skip((page - 1) * 5).Take(5);
                    else
                    {
                        LViewAgents.ItemsSource = currentAgents1.OrderBy(p => p.Title).ToList().Skip((page - 1) * 5).Take(5);
                    }
                }
                else if (SortCB.SelectedIndex == 1)
                {
                    if (isFlipped)
                        LViewAgents.ItemsSource = currentAgents1.OrderByDescending(p => p.Discount).ToList().Skip((page - 1) * 5).Take(5);
                    else
                    {
                        LViewAgents.ItemsSource = currentAgents1.OrderBy(p => p.Discount).ToList().Skip((page - 1) * 5).Take(5);
                    }
                }
                else if (SortCB.SelectedIndex == 2)
                {
                    if (isFlipped)
                        LViewAgents.ItemsSource = currentAgents1.OrderByDescending(p => p.Priority).ToList().Skip((page - 1) * 5).Take(5);
                    else
                        LViewAgents.ItemsSource = currentAgents1.OrderBy(p => p.Priority).ToList().Skip((page - 1) * 5).Take(5);
                }
            }
        }

        private void TypeFilterCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePage();
        }

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePage();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            isFlipped = !isFlipped;

            if (isFlipped)
            {
                SortWay.Content = "↑";
            }
            else
            {
                SortWay.Content = "↓";
            }
            UpdatePage();

        }

        private void previous_Click(object sender, RoutedEventArgs e)
        {
            if (page > 1) page--;
            lable_page.Content = page.ToString();
            UpdatePage();
        }

        private void next_Click(object sender, RoutedEventArgs e)
        {
            if (page != maxPage) page++;
            lable_page.Content = page.ToString();
            UpdatePage();
        }

        private void LViewAgents_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            using (DBEntities context = new DBEntities())
            {
                dynamic selectedData = LViewAgents.SelectedItem;
                if (selectedData != null)
                {
                    int agentID = selectedData.ID;
                    var selectedAgent = context.Agent.First(a => a.ID == agentID);
                    Window parentWindow = Window.GetWindow(this);
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    Frame frame = LogicalTreeHelper.FindLogicalNode(parentWindow, "MainFrame") as Frame;
                    frame.Navigate(new addEdit(selectedAgent, this));
                }
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            Frame frame = LogicalTreeHelper.FindLogicalNode(parentWindow, "MainFrame") as Frame;
            StartPage thisPage = frame.Content as StartPage;
            frame.Navigate(new addEdit(null, thisPage));
        }
    }
}
