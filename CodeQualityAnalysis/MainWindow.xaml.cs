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
        private Extractor _extractor;

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
        
            _extractor = new Extractor(fileDialog.FileName);

            FillTree();
        }

        /// <summary>
        /// Fill tree with module, types and methods and TODO: fields
        /// </summary>
        private void FillTree()
        {
            var itemModule = new TreeViewItem() { Header = _extractor.MainModule.Name };
            definitionTree.Items.Add(itemModule);

            foreach (var type in _extractor.MainModule.Types)
            {
                var itemType = new TreeViewItem() { Header = type.FullName };
                itemModule.Items.Add(itemType);

                foreach (var method in type.Methods)
                {
                    var itemMethod = new TreeViewItem() { Header = method.Name };
                    itemType.Items.Add(itemMethod);
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
                var type = (from t in this._extractor.MainModule.Types
                            where t.FullName == name
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
