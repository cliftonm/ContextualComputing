using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Clifton.Meaning;

/*
    Contextual data
    Inverting the object model so that data retains its context.
*/

namespace MeaningExplorer
{
    public class Explorer
    {
        // private EmployeeContractContext employeeContractContext;

        public void InitializeHomePage()
        {
            // var context = new EmployeeContractContext();
            // var context = new EmployeeContext(); 
            // var context = new AddressBookContext();
            // var context = new ParentChildContext();

            // var context = new ParentContext();
            // var context = new FatherContext();
            var context = new ChildContext();
            // var context = new PersonContext();

            Parser parser = new Parser();
            parser.Log = msg => Console.WriteLine(msg);
            try
            {
                parser.Parse(context);
                Console.WriteLine("==============");

                if (parser.AreDeclarationsValid)
                {
                    ShowGroups(parser.Groups);
                    string html = Renderer.CreatePage(parser);
                    File.WriteAllText("index.html", html);
                }
                else
                {
                    Console.WriteLine("Context is not valid!");
                    File.WriteAllText("index.html", "<p>Context declarations are not valid.  Missing entities:</p>" +
                        String.Join("<br>", parser.MissingDeclarations.Select(t => t.Name)));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Context is not valid!");
                string html = "<p>Context declarations are not valid.</p><p>" + ex.Message + "</p>";
                html = html + "<p>" + ex.StackTrace.Replace("\r\n", "<br>");
                File.WriteAllText("index.html", html);
            }
        }

        protected void ShowGroups(IReadOnlyList<Group> groups)
        {
            foreach (var group in groups)
            {
                Console.WriteLine("Group:");
                Console.WriteLine(group.Name + " => " + group.ContextPath.AsStringList());

                foreach (var field in group.Fields)
                {
                    Console.WriteLine("  " + field.Label + " => " + field.ContextPath.AsStringList());
                }

                Console.WriteLine();
            }
        }
    }
}
