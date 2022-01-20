using DevExpress.Xpf.Grid;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DXSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            feedStatusTreeList.Loaded += FeedStatusTreeList_Loaded;
        }
        DataTable dt = new DataTable();

        private void FeedStatusTreeList_Loaded(object sender, RoutedEventArgs e)
        {
            dt.Columns.Add("EventLogID", typeof(int));
            dt.Columns.Add("ParentID", typeof(int));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("state", typeof(string));
            dt.Rows.Add(new object[] { 0, 0, "Root" });
            dt.Rows.Add(new object[] { 1, 0, "Child 1" });
            dt.Rows.Add(new object[] { 2, 1, "Child 2" });
            dt.Rows.Add(new object[] { 3, 1, "Child 3" });
            dt.Rows.Add(new object[] { 4, 1, "Child 4" });
            dt.Rows.Add(new object[] { 5, 2, "Child 2-1" });

            dt.Rows.Add(new object[] { 6, 3, "Child 3-1" });
            dt.Rows.Add(new object[] { 7, 2, "Child 2-2" });
            dt.Rows.Add(new object[] { 9, 7, "Child 2-2-1" });
            dt.Rows.Add(new object[] { 10, 7, "Child 2-2-2" });
            dt.Rows.Add(new object[] { 8, 1, "Child 5" });

            feedStatusTreeListView.AutoPopulateServiceColumns = false;
            feedStatusTreeListView.KeyFieldName = "EventLogID";
            feedStatusTreeListView.ParentFieldName = "ParentID";
            feedStatusTreeList.AutoGenerateColumns = DevExpress.Xpf.Grid.AutoGenerateColumnsMode.AddNew;
            dt.AcceptChanges();
            feedStatusTreeList.ItemsSource = dt;
            feedStatusTreeListView.ExpandAllNodes();
            feedStatusTreeListView.BestFitColumns();
        }




        private void feedStatusTreeList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Return:
                    //{
                    //    e.Handled = true;
                    //    AddNewRow(this.gcSubtype, LogicalName, LogicalOrder);
                    //    BeginShowEditor((gcvSubtype.FocusedNode.Content as NodeContent)?.Id);
                    //}
                    break;

                case Key.Delete:
                    //{
                    //    DeleteRow();
                    //}
                    break;
                case Key.Tab:
                    {
                        var focusedNode = feedStatusTreeListView.FocusedNode;

                        KeyStates state = System.Windows.Input.Keyboard.GetKeyStates(Key.LeftShift);
                        if ((state & KeyStates.Down) == KeyStates.Down)
                        {
                            //feedStatusTreeListView.OutdentNode(focusedNode);
                            OutdentNode(focusedNode);
                        }
                        else
                        {
                            //feedStatusTreeListView.IndentNode(focusedNode);
                            IndentNode(focusedNode);
                        }

                        feedStatusTreeList.View.PostEditor();
                        feedStatusTreeList.View.HideEditor();
                        e.Handled = true;
                    }
                    break;
            }
        }

        /// <summary>
        /// IndentNode 与当前节点的前一个节点的level做比较
        /// 1. 当前节点的level - 前一个节点的level <= 1 时，可以缩进；
        /// 2. 当前节点缩进后，当前节点及其子节点同步缩进
        /// </summary>
        /// <param name="focusedNode"></param>
        private void IndentNode(TreeListNode focusedNode)
        {
            // 查找当前节点 同一层级的前一个节点
            var parentNode = focusedNode.ParentNode;
            var siblingNodes = parentNode.Nodes;
            TreeListNode previousNode;
            if (siblingNodes.IndexOf(focusedNode) == 0)
                previousNode = focusedNode.ParentNode;
            else
                previousNode = siblingNodes[siblingNodes.IndexOf(focusedNode) - 1]; // 当前节点同一层级的前一个节点，作为缩进后的父节点
            if (focusedNode.Level - previousNode.Level == 1)
                return;
            var parentRowView = previousNode.Content as DataRowView;
            var focusedRowView = focusedNode.Content as DataRowView;
            focusedRowView.Row["ParentID"] = parentRowView["EventLogID"];
            feedStatusTreeListView.FocusedNode.ParentNode.ExpandAll();

            if (previousNode.Nodes.Count > 0)
            {
                var newPreviousNode = previousNode.Nodes.LastOrDefault();
                // 插入到 newPreviousNode 的所有子节点的后面
                var node = GetLastChildNode(newPreviousNode);
                var newIndex = dt.Rows.IndexOf((node.Content as DataRowView).Row);
                var dr = dt.NewRow();
                dr.ItemArray = focusedRowView.Row.ItemArray;
                dt.Rows.Remove(focusedRowView.Row);
                dt.Rows.InsertAt(dr, newIndex);
                dr.AcceptChanges();
                dr["state"] = "1";
                DataRowView tempRowView = GetRowView(dr);
                feedStatusTreeListView.MoveFocusedRow(feedStatusTreeListView.GetNodeVisibleIndex(feedStatusTreeListView.GetNodeByContent(tempRowView)));
                feedStatusTreeListView.ExpandNode(feedStatusTreeListView.GetNodeVisibleIndex(feedStatusTreeListView.GetNodeByContent(tempRowView)));
            }
            PrintRows(dt);
        }

        private TreeListNode GetLastChildNode(TreeListNode currentNode)
        {
            var node = currentNode.Nodes.LastOrDefault();
            if (null == node)
                return currentNode;
            return GetLastChildNode(node);
        }
        private DataRowView GetRowView(DataRow dr)
        {
            DataRowView tempRowView = null;
            for (int i = 0; i < dt.DefaultView.Count; i++)
            {
                if (dt.DefaultView[i].Row == dr)
                    tempRowView = dt.DefaultView[i];
            }
            return tempRowView;
        }
        /// <summary>
        /// OutdentNode 当前节点的前一个节点
        /// 
        /// </summary>
        /// <param name="focusedNode"></param>
        private void OutdentNode(TreeListNode focusedNode)
        {
            if (focusedNode.Level == 0)
                return;

            // 查找当前节点 同一层级的前一个节点
            var parentNode = focusedNode.ParentNode;
            var siblingNodes = parentNode.Nodes;
            TreeListNode previousNode;
            if (siblingNodes.IndexOf(focusedNode) == 0)
                previousNode = focusedNode.ParentNode;
            else
                previousNode = siblingNodes[siblingNodes.IndexOf(focusedNode) - 1]; // 当前节点同一层级的前一个节点，作为缩进后的父节点
            // 同一层级后面的节点作为当前节点的子节点
            var newParentNode = parentNode.ParentNode;
            var focusedRow = (focusedNode.Content as DataRowView).Row;
            focusedRow["ParentID"] = newParentNode == null ? 0 : (newParentNode.Content as DataRowView)["EventLogID"];
            // 当前节点的所有兄弟节点作为其子节点
            if (siblingNodes.IndexOf(focusedNode) + 1 < siblingNodes.Count)
            {
                var firstSiblingIndex = dt.Rows.IndexOf((siblingNodes[siblingNodes.IndexOf(focusedNode) + 1]?.Content as DataRowView).Row);
                foreach (var chilNode in focusedNode.Nodes)
                {
                    var childRow = dt.NewRow();
                    var row = (chilNode.Content as DataRowView).Row;
                    childRow.ItemArray = row.ItemArray;
                    dt.Rows.Remove(row);
                    dt.Rows.InsertAt(childRow, firstSiblingIndex);
                    childRow.AcceptChanges();
                    firstSiblingIndex++;
                }
            }

            for (int i = siblingNodes.IndexOf(focusedNode) + 1; i < siblingNodes.Count; i++)
            {
                var node = siblingNodes[i];
                (node.Content as DataRowView).Row["ParentID"] = focusedRow["EventLogID"];
            }


            var ptRowView = previousNode.Content as DataRowView;
            var focusedRowView = focusedNode.Content as DataRowView;
            int index = dt.Rows.IndexOf(ptRowView.Row);
            var dr = dt.NewRow();
            dr.ItemArray = focusedRowView.Row.ItemArray;
            dt.Rows.Remove(focusedRowView.Row);
            dt.Rows.InsertAt(dr, index + 1);
            dr.AcceptChanges();
            dr["state"] = "1";
            feedStatusTreeListView.PostEditor();
            feedStatusTreeList.RefreshData();
            DataRowView tempRowView = null;
            for (int i = 0; i < dt.DefaultView.Count; i++)
            {
                if (dt.DefaultView[i].Row == dr)
                    tempRowView = dt.DefaultView[i];
            }

            feedStatusTreeListView.MoveFocusedRow(feedStatusTreeListView.GetNodeVisibleIndex(feedStatusTreeListView.GetNodeByContent(tempRowView)));
            feedStatusTreeListView.ExpandNode(feedStatusTreeListView.GetNodeVisibleIndex(feedStatusTreeListView.GetNodeByContent(tempRowView)));
            PrintRows(dt);
        }

        private void PrintRows(DataTable dt)
        {
            Console.WriteLine($"************************Begin**********************************");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Console.WriteLine($"ParentID: {dt.Rows[i]["ParentID"] },EventLogID：{dt.Rows[i]["EventLogID"]}，" +
                    $"Name：{dt.Rows[i]["Name"]}, RowState: {dt.Rows[i].RowState}");
            }
            Console.WriteLine($"************************End**********************************\n");
        }
    }
}
