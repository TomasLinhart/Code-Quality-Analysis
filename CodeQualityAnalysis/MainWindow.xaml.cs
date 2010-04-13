using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Mono.Cecil;
using Mono.Cecil.Cil;
using QuickGraph;
using QuickGraph.Collections;

namespace CodeQualityAnalysis
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MetricsReader _metricsReader;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnOpenAssembly_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new Microsoft.Win32.OpenFileDialog
                                 {
                                     Filter = "Component Files (*.dll, *.exe)|*.dll;*.exe"
                                 };

            fileDialog.ShowDialog();

            if (String.IsNullOrEmpty(fileDialog.FileName))
                return;

            definitionTree.Items.Clear();
        
            _metricsReader = new MetricsReader(fileDialog.FileName);

            FillTree();
        }

        /// <summary>
        /// Fill tree with module, types and methods and TODO: fields
        /// </summary>
        private void FillTree()
        {
            var itemModule = new TreeViewItem() { Header = _metricsReader.MainModule.Name };
            definitionTree.Items.Add(itemModule);

            foreach (var ns in _metricsReader.MainModule.Namespaces)
            {
                var nsType = new TreeViewItem() { Header = ns.Name };
                itemModule.Items.Add(nsType);

                foreach (var type in ns.Types)
                {
                    var itemType = new TreeViewItem() { Header = type.Name };
                    nsType.Items.Add(itemType);

                    foreach (var method in type.Methods)
                    {
                        var itemMethod = new TreeViewItem() { Header = method.Name };
                        itemType.Items.Add(itemMethod);
                    }

                    foreach (var field in type.Fields)
                    {
                        var itemField = new TreeViewItem() { Header = field.Name };
                        itemType.Items.Add(itemField);
                    }
                }
            }
        }

        private void definitionTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = definitionTree.SelectedItem as TreeViewItem;

            if (item != null)
            {
                // would be better inherit from TreeViewItem and add reference into it
                // will do it later or will use another tree maybe tree from SharpDevelop
                string name = item.Header.ToString();
                txbTypeInfo.Text = "Infobox: \n" + name;
                var type = (from n in this._metricsReader.MainModule.Namespaces
                            from t in n.Types
                            where t.Name == name
                            select t).SingleOrDefault();

                if (type != null)
                {
                    var graph = CreateGraphForType(type);
                    if (graph.VertexCount > 0)
                    {
                        graphLayout.Graph = graph;
                    }
                }
            }

        }

        private BidirectionalGraph<object, IEdge<object>> CreateGraphForType(Type type)
        {
            var g = new BidirectionalGraph<object, IEdge<object>>();

            foreach (var method in type.Methods)
            {
                g.AddVertex(method.Name);
            }

            foreach (var method in type.Methods)
            {
                foreach (var methodUse in method.MethodUses)
                {
                    g.AddEdge(new Edge<object>(method.Name, methodUse.Name));
                }
            }            

            return g;
        }
    }
}
