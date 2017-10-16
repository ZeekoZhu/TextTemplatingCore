using System;
using TextTemplating;
using TextTemplating.Infrastructure;
using TextTemplating.T4.Parsing;
using TextTemplating.T4.Preprocessing;

namespace RuntimeTemplateSample
{
    public partial class ApiClientTemplate : TextTransformationBase
    {
        public override string TransformText()
        {
            WriteLine();

            foreach (var api in Apis)
            {
                WriteApi(api.Url, api.Method);
            }
            WriteLine();
            WriteLine();
            WriteLine();


            return GenerationEnvironment.ToString();
        }

        private void WriteApi(string method, string url)
        {
            WriteLine();
            Write("function GetValues(callback) {");
            WriteLine();
            Write("    $.ajax({");
            WriteLine();
            Write("        url: \"");
            Write((url).ToString()); Write("\",");
            WriteLine();
            Write("        type: \"");
            Write((method).ToString()); Write("\",");
            WriteLine();
            Write("        data: JSON.stringify(obj),");
            WriteLine();
            Write("        contentType: \"application/json\",");
            WriteLine();
            Write("        success: function (res) {");
            WriteLine();
            Write("            callback(res);");
            WriteLine();
            Write("        }");
            WriteLine();
            Write("    })");
            WriteLine();
            Write("}");
            WriteLine();

        }
    }
}
