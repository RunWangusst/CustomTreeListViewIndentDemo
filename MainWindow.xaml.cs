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
            dt.Rows.Add(new object[] { 0, -1, "Root" });
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
            if (focusedNode.VisibleIndex == 0)
                return;
            var parentNode = focusedNode.ParentNode;
            var siblingNodes = null == parentNode ? feedStatusTreeListView.Nodes : parentNode.Nodes;
            if (siblingNodes.IndexOf(focusedNode) == 0)
                return;
            // 查找当前节点的上一个节点（作为当前节点的父节点）
            var previousNode = siblingNodes[siblingNodes.IndexOf(focusedNode) - 1]; // 当前节点同一层级的前一个节点，作为缩进后的父节点

            var focusedRowView = focusedNode.Content as DataRowView;
            focusedRowView.Row["ParentID"] = (previousNode.Content as DataRowView)["EventLogID"];
            feedStatusTreeListView.FocusedNode.ParentNode.ExpandAll();

            var newPreviousNode = previousNode.Nodes.Count > 0 ? previousNode.Nodes.LastOrDefault() : previousNode;
            // 插入到 newPreviousNode 的所有子节点的后面
            var node = GetLastChildNode(newPreviousNode);
            var drr = newPreviousNode.Content as DataRowView;
            //int newIndex = feedStatusTreeListView.GetNodeVisibleIndex(feedStatusTreeListView.GetNodeByContent(drr));
            var focusedIndex = dt.Rows.IndexOf(focusedRowView.Row);
            var newIndex = dt.Rows.IndexOf(drr.Row);
            var dr = dt.NewRow();
            dr.ItemArray = focusedRowView.Row.ItemArray;
            dt.Rows.Remove(focusedRowView.Row);
            dt.Rows.InsertAt(dr, focusedIndex < newIndex ? newIndex : newIndex + 1);
            dr.AcceptChanges();
            dr["state"] = "1";
            DataRowView tempRowView = null;
            for (int i = 0; i < dt.DefaultView.Count; i++)
            {
                if (dt.DefaultView[i].Row == dr)
                    tempRowView = dt.DefaultView[i];
            }
            int currentNodeIndex = feedStatusTreeListView.GetNodeVisibleIndex(feedStatusTreeListView.GetNodeByContent(tempRowView));
            feedStatusTreeListView.MoveFocusedRow(currentNodeIndex);
            feedStatusTreeListView.ExpandNode(currentNodeIndex);
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
            PrintRows(dt);
            if (focusedNode.Level == 0)
                return;
            // 找到 CurrentNode 当前节点的父节点作为该节点的上一个节点 PreviousNode 
            var previousNode = focusedNode.ParentNode;
            TreeListNode parentNode = previousNode.ParentNode;
            // 同一层级后面的节点作为当前节点的子节点
            var focusedRow = (focusedNode.Content as DataRowView).Row;
            focusedRow["ParentID"] = parentNode == null ? -1 : (parentNode.Content as DataRowView)["EventLogID"];
            feedStatusTreeListView.FocusedNode.ParentNode?.ExpandAll();
            // 当前节点的所有兄弟节点作为其子节点
            var siblingNodes = previousNode.Nodes;
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
            int focusedIndex = dt.Rows.IndexOf(focusedRowView.Row);
            int index = dt.Rows.IndexOf(ptRowView.Row);
            var dr = dt.NewRow();
            dr.ItemArray = focusedRowView.Row.ItemArray;
            dt.Rows.Remove(focusedRowView.Row);
            dt.Rows.InsertAt(dr, focusedIndex < index ? index : index + 1);
            dr.AcceptChanges();
            dr["state"] = "1";
            DataRowView tempRowView = GetRowView(dr);
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
