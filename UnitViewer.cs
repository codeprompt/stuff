using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CodeConics.Units;
using System.Windows.Forms;
using System.Drawing;

namespace TreeViewer
{
    public class UnitViewer
    {
        static ViewForm form = new ViewForm();
        public static void View(SyntaxNode sn)
        {
            form = new ViewForm() { Text = "Unit hierarchy" };
            form.treeView.Nodes.Add(getNode(sn));
            Application.Run(form);
        }

        static TreeNode getNode(Node node)
        {
            if (node is TokenNode)
            {
                TreeNode n = new TreeNode((node as TokenNode).Code);
                n.BackColor = Color.Purple;
                n.ForeColor = Color.White;
                return n;
            }
            else if (node is SyntaxNode)
            {
                List<TreeNode> children = new List<TreeNode>();

                var ns = (node as SyntaxNode).ToList();
                foreach (Node n in ns)
                {
                    children.Add(getNode(n));
                }

                TreeNode o = new TreeNode((node as SyntaxNode).Name, children.ToArray());
                o.BackColor = Color.Orange;
                return o;
            }
            else return null;
        }
    }
}
