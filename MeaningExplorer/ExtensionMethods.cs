/* The MIT License (MIT)
* 
* Copyright (c) 2018 Marc Clifton
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

/* Code Project Open License (CPOL) 1.02
* https://www.codeproject.com/info/cpol10.aspx
*/

using System.Text;

using Clifton.Core.ExtensionMethods;

namespace MeaningExplorer
{
    public static class ExtensionMethods
    {
        public static StringBuilder StartHtml(this StringBuilder sb)
        {
            sb.Append("<html>");

            return sb;
        }

        public static StringBuilder EndHtml(this StringBuilder sb)
        {
            sb.Append("</html>");

            return sb;
        }

        public static StringBuilder StartHead(this StringBuilder sb)
        {
            sb.Append("<head>");

            return sb;
        }

        public static StringBuilder EndHead(this StringBuilder sb)
        {
            sb.Append("</head>");

            return sb;
        }

        public static StringBuilder StartScript(this StringBuilder sb)
        {
            sb.Append("<script>");

            return sb;
        }

        public static StringBuilder EndScript(this StringBuilder sb)
        {
            sb.Append("</script>");

            return sb;
        }

        public static StringBuilder Javascript(this StringBuilder sb, string js)
        {
            sb.Append(js);

            return sb;
        }

        public static StringBuilder Stylesheet(this StringBuilder sb, string href)
        {
            sb.Append("<link rel='stylesheet' href=" + href.SingleQuote() + ">");

            return sb;
        }

        public static StringBuilder Script(this StringBuilder sb, string src, string type= "type='text/javascript'")
        {
            sb.Append("<script " + type+" src=" + src.SingleQuote() + "></script>");

            return sb;
        }

        public static StringBuilder CustomScript(this StringBuilder sb, string src)
        {
            sb.Append("<script>"+src+"</script>");

            return sb;
        }

        public static StringBuilder CustomStyle(this StringBuilder sb, string src)
        {
            sb.Append("<style>" + src + "</style>");

            return sb;
        }

        public static StringBuilder StartBody(this StringBuilder sb)
        {
            sb.Append("<body>");

            return sb;
        }

        public static StringBuilder EndBody(this StringBuilder sb)
        {
            sb.Append("</body>");

            return sb;
        }

        public static StringBuilder StartDiv(this StringBuilder sb)
        {
            sb.Append("<div>");

            return sb;
        }

        public static StringBuilder StartInlineDiv(this StringBuilder sb)
        {
            // no horizontal space between divs.
            // use display:inline-block to create a little space between divs.
            sb.Append("<div style='display:inline; float:left'>");

            return sb;
        }

        public static StringBuilder EndDiv(this StringBuilder sb)
        {
            sb.Append("</div>");

            return sb;
        }

        public static StringBuilder StartParagraph(this StringBuilder sb)
        {
            sb.Append("<p>");

            return sb;
        }

        public static StringBuilder EndParagraph(this StringBuilder sb)
        {
            sb.Append("</p>");

            return sb;
        }

        public static StringBuilder StartFieldSet(this StringBuilder sb)
        {
            sb.Append("<fieldset>");

            return sb;
        }

        public static StringBuilder EndFieldSet(this StringBuilder sb)
        {
            sb.Append("</fieldset>");

            return sb;
        }

        public static StringBuilder Legend(this StringBuilder sb, string name)
        {
            sb.Append("<legend>");
            sb.Append(name);
            sb.Append("</legend>");

            return sb;
        }

        public static StringBuilder AppendLabel(this StringBuilder sb, string label)
        {
            sb.Append(label);

            return sb;
        }

        public static StringBuilder AppendTextInput(this StringBuilder sb)
        {
            sb.Append("<input type=\"text\">");

            return sb;
        }

        public static StringBuilder LineBreak(this StringBuilder sb)
        {
            sb.Append("<br>");

            return sb;
        }

        public static StringBuilder StartTable(this StringBuilder sb)
        {
            sb.Append("<table>");

            return sb;
        }

        public static StringBuilder EndTable(this StringBuilder sb)
        {
            sb.Append("</table>");

            return sb;
        }

        public static StringBuilder StartRow(this StringBuilder sb)
        {
            sb.Append("<tr>");

            return sb;
        }

        public static StringBuilder NextRow(this StringBuilder sb)
        {
            sb.Append("</tr><tr>");

            return sb;
        }

        public static StringBuilder EndRow(this StringBuilder sb)
        {
            sb.Append("<tr>");

            return sb;
        }

        public static StringBuilder StartHeader(this StringBuilder sb)
        {
            sb.Append("<th>");

            return sb;
        }

        public static StringBuilder NextHeader(this StringBuilder sb)
        {
            sb.Append("</th><th>");

            return sb;
        }

        public static StringBuilder EndHeader(this StringBuilder sb)
        {
            sb.Append("</th>");

            return sb;
        }

        public static StringBuilder StartColumn(this StringBuilder sb)
        {
            sb.Append("<td>");

            return sb;
        }

        public static StringBuilder NextColumn(this StringBuilder sb)
        {
            sb.Append("</td><td>");

            return sb;
        }

        public static StringBuilder EndColumn(this StringBuilder sb)
        {
            sb.Append("</td>");

            return sb;
        }

        public static StringBuilder StartSelect(this StringBuilder sb)
        {
            sb.Append("<select>");

            return sb;
        }

        public static StringBuilder EndSelect(this StringBuilder sb)
        {
            sb.Append("</select>");

            return sb;
        }

        public static StringBuilder OnChange(this StringBuilder sb, string fncCall)
        {
            return sb.CustomAttribute("onchange", fncCall);
        }

        public static StringBuilder Option(this StringBuilder sb, string text, string val = "")
        {
            if (val == string.Empty)
            {
                sb.Append("<option>" + text + "</option>");
            }
            else
            {
                sb.Append("<option value=" + val.SingleQuote() + ">" + text + "</option>");
            }

            return sb;
        }

        public static StringBuilder StartButton(this StringBuilder sb)
        {
            sb.Append("<button type='button'>");

            return sb;
        }

        public static StringBuilder EndButton(this StringBuilder sb)
        {
            sb.Append("</button>");

            return sb;
        }

        /// <summary>
        /// Inject a "class='[tag]'" into the last element.
        /// </summary>
        public static StringBuilder Class(this StringBuilder sb, string tagValue, bool conditional = true)
        {
            if (conditional)
            {
                StringBuilder newsb = InsertAttribute(sb, "class", tagValue);

                // Return the same StringBuilder instance.
                // TODO: This is rather ugly.  We might consider the class I use in TemporalAgency to pass in attributes as parameters.
                sb.Clear();
                sb.Append(newsb);
            }

            return sb;
        }

        /// <summary>
        /// Inject a "class='[tag]'" into the last element.
        /// </summary>
        public static StringBuilder ID(this StringBuilder sb, string tagValue)
        {
            StringBuilder newsb = InsertAttribute(sb, "id", tagValue);

            // Return the same StringBuilder instance.
            // TODO: This is rather ugly.  We might consider the class I use in TemporalAgency to pass in attributes as parameters.
            sb.Clear();
            sb.Append(newsb);

            return sb;
        }

        public static StringBuilder CustomStyle(this StringBuilder sb, string attrName, string val)
        {
            StringBuilder newsb = InsertAttribute(sb, "style", attrName + ":" + val);

            // Return the same StringBuilder instance.
            // TODO: This is rather ugly.  We might consider the class I use in TemporalAgency to pass in attributes as parameters.
            sb.Clear();
            sb.Append(newsb);

            return sb;
        }

        /// <summary>
        /// Inject a "class='[tag]'" into the last element.
        /// </summary>
        public static StringBuilder CustomAttribute(this StringBuilder sb, string attributeName, string tagValue)
        {
            StringBuilder newsb = InsertAttribute(sb, attributeName, tagValue);

            // Return the same StringBuilder instance.
            // TODO: This is rather ugly.  We might consider the class I use in TemporalAgency to pass in attributes as parameters.
            sb.Clear();
            sb.Append(newsb);

            return sb;
        }

        private static StringBuilder InsertAttribute(StringBuilder sb, string attr, string tag)
        {
            StringBuilder sbRet = new StringBuilder();
            string current = sb.ToString();
            string leftOfLastElement = current.LeftOfRightmostOf("<");
            string rightOfLastElement = current.RightOfRightmostOf("<").RightOf(">");
            string middle = current.RightOfRightmostOf("<").LeftOf(">");

            // If the tag already exists, prepend the new attribute, separated by a semicolon.
            // TODO: This would be easier if we just built a collection of tags, etc., and then generated the final string,
            // rather than manipulating the string as we go.
            if (middle.Contains(attr + "="))
            {
                string leftMiddle = middle.LeftOf(attr + "='");
                string rightMiddle = middle.RightOf(attr + "='");
                sbRet.Append(leftOfLastElement);
                sbRet.Append("<");
                sbRet.Append(leftMiddle);
                sbRet.Append(attr + "='" + tag + "'; ");
                sbRet.Append(rightMiddle);
                sbRet.Append(">");
                sbRet.Append(rightOfLastElement);
            }
            else
            {
                sbRet.Append(leftOfLastElement);
                sbRet.Append("<");
                sbRet.Append(middle);
                sbRet.Append(" " + attr + "=" + tag.SingleQuote());
                sbRet.Append(">");
                sbRet.Append(rightOfLastElement);
            }

            return sbRet;
        }
    }
}
