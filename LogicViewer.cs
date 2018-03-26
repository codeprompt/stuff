using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CodeConics.Syntactical;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;

namespace TreeViewer
{
    public class LogicViewer
    {
        static ViewForm form = new ViewForm() { Text = "Working Grammer set (EBNF)" };
        public static void View()
        {
            form = new ViewForm() { Text = "Working Grammer set (EBNF)" };
            ParserGrammer grammer = new ParserGrammer();

            Dictionary<int, List<string>> dict = new Dictionary<int, List<string>>();
            foreach (KeyValuePair<string, Production> kvp in grammer.PRODUCTIONS)
            {
                if (dict.ContainsKey(kvp.Value.PrecedenceIndex))
                {
                    dict[kvp.Value.PrecedenceIndex].Add(kvp.Key);
                }
                else
                {
                    dict.Add(kvp.Value.PrecedenceIndex, new List<string>());
                    dict[kvp.Value.PrecedenceIndex].Add(kvp.Key);
                }
            }
            foreach (KeyValuePair<int, List<string>> kvp in dict)
            {
                List<TreeNode> ns = new List<TreeNode>();
                List<string> li = kvp.Value;
                foreach (string s in li)
                {
                    string name = s;
                    if (grammer.ASSOCIATIVITY[kvp.Key] == 0)
                        name = "(L)    " + name;
                    else if (grammer.ASSOCIATIVITY[kvp.Key] == 1)
                        name = "(N)    " + name;
                    else if (grammer.ASSOCIATIVITY[kvp.Key] == 2)
                        name = "(R)    " + name;

                    List<TreeNode> ds = getDerivations(grammer.PRODUCTIONS[s]);

                    TreeNode n = new TreeNode(name, ds.ToArray());
                    ns.Add(n);
                }

                TreeNode node = new TreeNode(kvp.Key.ToString(), ns.ToArray());
                form.treeView.Nodes.Add(node);
            }

            foreach (TreeNode tnde in form.treeView.Nodes)
            {
                tnde.ExpandAll();
            }
            new Thread(() => form.ShowDialog()).Start();
        }

        static List<TreeNode> getDerivations(Production prod)
        {
            List<TreeNode> ns = new List<TreeNode>();
            foreach (Derivation der in prod.Derivations)
            {
                List<TreeNode> cns = new List<TreeNode>();
                bool tempindent = false;
                string indent = "";
                foreach (Symbol sym in der.Symbols)
                {
                    if (sym is MetaSymbol)
                    {
                        if (sym is OpenSquareSymbol)
                        {
                            string code = "[   ";
                            if (tempindent)
                            {
                                code = "          " + code;
                                tempindent = false;
                            }
                            TreeNode tnd = new TreeNode(indent + code);
                            cns.Add(tnd);
                            indent += "          ";
                        }
                        else if (sym is CloseSquareSymbol)
                        {
                            string code = "]   ";
                            if (indent != "") indent = indent.Substring(0, indent.Length - 10);
                            TreeNode tnd = new TreeNode(indent + code);
                            cns.Add(tnd);
                        }
                        else if (sym is OpenRoundSymbol)
                        {
                            string code = "(   ";
                            if (tempindent)
                            {
                                code = "          " + code;
                                tempindent = false;
                            }
                            TreeNode tnd = new TreeNode(indent + code);
                            cns.Add(tnd);
                            indent += "          ";
                        }
                        else if (sym is CloseRoundSymbol)
                        {
                            string code = ")   ";
                            if (indent != "") indent = indent.Substring(0, indent.Length - 10);
                            TreeNode tnd = new TreeNode(indent + code);
                            cns.Add(tnd);
                        }
                        else if (sym is OpenCurlySymbol)
                        {
                            string code = "{   ";
                            if (tempindent)
                            {
                                code = "          " + code;
                                tempindent = false;
                            }
                            TreeNode tnd = new TreeNode(indent + code);
                            cns.Add(tnd);
                            indent += "          ";
                        }
                        else if (sym is CloseCurlySymbol)
                        {
                            string code = "}   ";
                            if (indent != "") indent = indent.Substring(0, indent.Length - 10);
                            TreeNode tnd = new TreeNode(indent + code);
                            cns.Add(tnd);
                        }
                        else if (sym is OrSymbol)
                        {
                            cns[cns.Count - 1].Text += "   |";
                            tempindent = true;
                        }
                        else if (sym is SkipSymbol)
                        {
                            string code = "<%skip>";
                            if (tempindent)
                            {
                                code = "          " + code;
                                tempindent = false;
                            }
                            TreeNode tnd = new TreeNode(indent + code);
                            cns.Add(tnd);
                        }
                    }
                    else if (sym is RealSymbol)
                    {
                        if (sym is TerminalSymbol) 
                        {
                            string code = (sym as TerminalSymbol).Role;
                            if (!string.IsNullOrEmpty((sym as TerminalSymbol).Code)
                                && !string.IsNullOrWhiteSpace((sym as TerminalSymbol).Code)) 
                                    code += "<\"" + (sym as TerminalSymbol).Code +"\">";
                            if ((sym as TerminalSymbol).Spaced) code += "<%s>";
                            if ((sym as TerminalSymbol).Negated) code = code + "<%!/>";
                            if (tempindent)
                            {
                                code = "          " + code;
                                tempindent = false;
                            }
                            TreeNode tnd = new TreeNode(indent + code);
                            cns.Add(tnd);
                        }
                        else if (sym is NonterminalSymbol) 
                        {
                            string code = "<" + (sym as NonterminalSymbol).Name + ">";
                            if ((sym as NonterminalSymbol).Spaced) code += "<%s>";
                            if ((sym as NonterminalSymbol).Negated) code = code + "<%!/>";
                            if ((sym as NonterminalSymbol).Dismantled) code = code + "<%?>";
                            if (tempindent)
                            {
                                code = "          " + code;
                                tempindent = false;
                            }
                            TreeNode tnd = new TreeNode(indent + code);
                            cns.Add(tnd);
                        }
                        else if (sym is ChoiceSymbol) 
                        {
                            var choice = sym as ChoiceSymbol;
                            for (int x = 0; x < choice.Symbols.Length; x++)
                            {
                                if (choice.Symbols[x] is TerminalSymbol)
                                {
                                    string code = (choice.Symbols[x] as TerminalSymbol).Role;
                                    if (!string.IsNullOrEmpty((choice.Symbols[x] as TerminalSymbol).Code)
                                        && !string.IsNullOrWhiteSpace((choice.Symbols[x] as TerminalSymbol).Code))
                                        code += "<\"" + (choice.Symbols[x] as TerminalSymbol).Code + "\">";
                                    if ((choice.Symbols[x] as TerminalSymbol).Spaced) code += "<%s>";
                                    if ((choice.Symbols[x] as TerminalSymbol).Negated) code = code + "<%!/>";
                                    if (tempindent) code = "          " + code;
                                    if (x < choice.Symbols.Length - 1)
                                    {
                                        code += "   |";
                                        tempindent = true;
                                    }
                                    else tempindent = false;
                                    TreeNode tnd = new TreeNode(indent + code);
                                    cns.Add(tnd);
                                }
                                else if (choice.Symbols[x] is NonterminalSymbol)
                                {
                                    string code = "<" + (choice.Symbols[x] as NonterminalSymbol).Name + ">";
                                    if ((choice.Symbols[x] as NonterminalSymbol).Spaced) code += "<%s>";
                                    if ((choice.Symbols[x] as NonterminalSymbol).Negated) code = code + "<%!/>";
                                    if ((choice.Symbols[x] as NonterminalSymbol).Dismantled) code = code + "<%?>";
                                    if (tempindent) code = "          " + code;
                                    if (x < choice.Symbols.Length - 1)
                                    {
                                        code += "   |";
                                        tempindent = true;
                                    }
                                    else tempindent = false;
                                    TreeNode tnd = new TreeNode(indent + code);
                                    cns.Add(tnd);
                                }
                            }
                        }
                    }
                }

                TreeNode node = new TreeNode("->  ", cns.ToArray());
                ns.Add(node);
            }
            return ns;
        }
    }
}
