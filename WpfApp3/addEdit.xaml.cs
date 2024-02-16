using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
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
    /// Логика взаимодействия для addEdit.xaml
    /// </summary>
    public partial class addEdit : Page
    {

        private Agent _currentAgent = new Agent();

        private StartPage mainWindow;
        List<AgentType> allTypes;

        public addEdit(Agent currentAgent, StartPage mainWindow)
        {
            InitializeComponent();
            LoadTypes();
            this.mainWindow = mainWindow;
            
            if (currentAgent != null)
            {
                _currentAgent = currentAgent;
                AgentType selectedAgentType = CBTypes.Items.Cast<AgentType>().FirstOrDefault(type => type.ID == _currentAgent.AgentTypeID);
                CBTypes.SelectedItem = selectedAgentType;
                DataContext = _currentAgent;

                BtnDel.Visibility = Visibility.Visible;
            }

            this.mainWindow = mainWindow;

        }

        private void LoadTypes()
        {
            using (DBEntities context = new DBEntities())
            {
                allTypes = context.AgentType.ToList();
                CBTypes.ItemsSource = allTypes;
            }
        }

        private void BtnSaveApp_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();
            if (AE_Title.Text.Length == 0 || CBTypes.SelectedItem == null || AE_INN.Text.Length == 0 || AE_Phone.Text.Length == 0)
            {
                errors.AppendLine("Заполните все поля.");
            }
            if (!(int.TryParse(AE_Priority.Text, out int i)))
            {
                errors.AppendLine("Приоритет должен быть числом.");
            }
            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            using (DBEntities context = new DBEntities())
            {
                AgentType selectedAgentType = (AgentType)CBTypes.SelectedItem;
                if (_currentAgent == null)
                {
                    int agentID = _currentAgent.ID;
                    Agent existingAgent = context.Agent.FirstOrDefault(a => a.ID == agentID);


                    // Проверяем, найден ли агент
                    if (existingAgent != null)
                    {
                        
                        existingAgent.Title = AE_Title.Text;
                        existingAgent.AgentTypeID = selectedAgentType.ID;
                        existingAgent.Address = AE_Address.Text;
                        existingAgent.INN = AE_INN.Text;
                        existingAgent.KPP = AE_KPP.Text;
                        existingAgent.DirectorName = AE_DirectorName.Text;
                        existingAgent.Phone = AE_Phone.Text;
                        existingAgent.Email = AE_Email.Text;
                        existingAgent.Logo = AE_Logo.Text;
                        existingAgent.Priority = Convert.ToInt32(AE_Priority.Text);

                        // Сохраняем изменения в базе данных
                        context.SaveChanges();
                    }

                    MessageBox.Show("Данные сохранены.");
                } else
                {
                    _currentAgent = new Agent() { Title = AE_Title.Text, AgentTypeID = selectedAgentType.ID, Address = AE_Address.Text, INN = AE_INN.Text, KPP = AE_KPP.Text, DirectorName = AE_DirectorName.Text, Phone = AE_Phone.Text, Email = AE_Email.Text, Logo = AE_Logo.Text, Priority = Convert.ToInt32(AE_Priority.Text) };
                    context.Agent.Add(_currentAgent);
                    context.SaveChanges();
                    MessageBox.Show("Агент успешно создан.");
                }
                    
            }
            if (CBTypes.SelectedItem != null)
            {
                AgentType selectedAgentType = (AgentType)CBTypes.SelectedItem;
                _currentAgent.AgentTypeID = selectedAgentType.ID;
            }

            Manager.MainFrame.GoBack();

            mainWindow.UpdatePage();

        }

        private void BtnDel_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"Вы точно хотите удалить данный элемент?", "Внимание",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (DBEntities context = new DBEntities())
                    {
                        Agent agentToDelete = context.Agent.FirstOrDefault(a => a.ID == _currentAgent.ID);
                       
                        if (agentToDelete != null)
                        {
                            var productSalesToDelete = context.ProductSale.Where(ps => ps.AgentID == agentToDelete.ID);
                            context.ProductSale.RemoveRange(productSalesToDelete);

                            context.Agent.Remove(agentToDelete);
                         
                            context.SaveChanges();
                        }
                    }                        

                    MessageBox.Show("Данные удалены.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
            Manager.MainFrame.GoBack();
            mainWindow.UpdatePage();
        }

        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.GoBack();
        }
    }
}
