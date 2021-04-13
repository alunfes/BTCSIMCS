using System;

using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace BTCSIM
{
    static public class LineChart
    {
        static public void DisplayLineChart(List<double> data, string title)
        {
            Console.WriteLine("displaying line chart...");
            Encoding enc = Encoding.GetEncoding("UTF-8");
            StreamWriter writer = new StreamWriter(@"./line_chart.html", false, enc);
            writer.WriteLine(@"<!DOCTYPE html>" + "\r\n" +
                @"<html lang=""ja"">" + "\r\n" +
                @"<head>" + "\r\n" +
                @"<meta charset = ""utf-8"">" + "\r\n" +
                @"<title> グラフ </title>" + "\r\n" +
                @"</head>" + "\r\n" +
                @"<body>" + "\r\n" +
                @"<h1>" + title + "</h1>" + "\r\n" +
                @"<canvas id=""myLineChart""></canvas>" + "\r\n" +

                @"<script src=""https://cdnjs.cloudflare.com/ajax/libs/Chart.js/2.7.2/Chart.bundle.js""></script>" + "\r\n" +
                @"<script>" + "\r\n" +
                @"var ctx=document.getElementById(""myLineChart"");" + "\r\n" +
                @"var myLineChart=new Chart(ctx, {" + "\r\n" +
                @"type: 'line'," + "\r\n" +
                @"data: {" + "\r\n" +
                GenerateNumericalLabel(data) +
                @"datasets: [" + "\r\n" +
                @"{" + "\r\n" +
                @"label: 'PL'," + "\r\n" +
                GenerateData(data) +
                @"borderColor: ""rgba(255,0,0,1)""," + "\r\n" +
                @"backgroundColor: ""rgba(0,0,0,0)""" + "\r\n" +
                @"}" + "\r\n" +
                @"]," + "\r\n" +
                @"}," + "\r\n" +
                @"options: {" + "\r\n" +
                @"title: {" + "\r\n" +
                @"display: true," + "\r\n" +
                @"text: 'PL Log'" + "\r\n" +
                @"}," + "\r\n" +
                @"scales: {" + "\r\n" +
                @"yAxes: [{" + "\r\n" +
                @"ticks: {" + "\r\n" +
                @"suggestedMax:" + data.Max().ToString() + ",\r\n" +
                @"suggestedMin:" + data.Min().ToString() + ",\r\n" +
                //@"stepSize: 10," + "\r\n"+
                @"callback: function(value, index, values){" + "\r\n" +
                @"return  value +  'usd'" + "\r\n" +
                @"}" + "\r\n" +
                @"}" + "\r\n" +
                @"}]" + "\r\n" +
                @"}," + "\r\n" +
                @"}" + "\r\n" +
                @"});" + "\r\n" +
                @"</script>" + "\r\n" +

                @"</body>" + "\r\n" +
                @"</html>" + "\r\n"
                );
            writer.Close();
            //System.Diagnostics.Process.Start(@"/Applications/Google Chrome.app", @"./line_chart.html");
            System.Diagnostics.Process.Start(@"/Applications/Google Chrome.app/Contents/MacOS/Google Chrome", @"./line_chart.html");
        }



        static public void DisplayLineChart2(List<double> data, List<double> close, List<double> buy, List<double> sell, string title)
        {
            Console.WriteLine("displaying line chart...");
            Encoding enc = Encoding.GetEncoding("UTF-8");
            StreamWriter writer = new StreamWriter(@"./line_chart.html", false, enc);
            writer.WriteLine(@"<!DOCTYPE html>" + "\r\n" +
                @"<html lang=""ja"">" + "\r\n" +
                @"<head>" + "\r\n" +
                @"<meta charset = ""utf-8"">" + "\r\n" +
                @"<title> グラフ </title>" + "\r\n" +
                @"</head>" + "\r\n" +
                @"<body>" + "\r\n" +
                @"<h1>" + title + "</h1>" + "\r\n" +
                @"<canvas id=""myLineChart""></canvas>" + "\r\n" +

                @"<script src=""https://cdnjs.cloudflare.com/ajax/libs/Chart.js/2.7.2/Chart.bundle.js""></script>" + "\r\n" +
                @"<script>" + "\r\n" +
                @"window.onload = function() {" + "\r\n" +
                @"ctx = document.getElementById('myLineChart').getContext('2d');" + "\r\n" +
                @"window.myBar = new Chart(ctx, {" + "\r\n" +
                @"type: 'bar'," + "\r\n" +
                @"data: barChartData," + "\r\n" +
                @"options: complexChartOption" + "\r\n" +
                @"});" + "\r\n" +
                @"};" + "\r\n" +
                @"</script>" + "\r\n" +

                @"<script>" + "\r\n" +
                @"var barChartData = {" + GenerateNumericalLabel(data) +
                @"datasets: [" + "\r\n" +
                //generatePlotChart(2, "buy", buy, new int[] { 0, 0, 255 }) +
                //generatePlotChart(2, "sell", sell, new int[] { 255, 0, 0 }) +
                generateChart("line", 1, "pl", data, new int[] {30,250,30 }) +
                generateChart("line", 2, "close", close, new int[] { 250, 30, 250 }) +
                @"]," + "\r\n" + "};" + "\r\n" +
                @"</script>" + "\r\n" +

                @"<script>" + "\r\n" +
                @"var complexChartOption = {" + "\r\n" +
                @"responsive: true," + "\r\n" +
                @"scales:{" + "\r\n" +
                @"yAxes:" + "\r\n" +
                @"[{id: ""y-axis-1""," + "\r\n" +
                @"type: ""linear"", " + "\r\n" +
                @"position: ""left""," + "\r\n" +
                @"ticks:{" + "\r\n" +
                @"max: " +data.Max().ToString()+","+ "\r\n" +
                @"min: " +data.Min().ToString() + "," + "\r\n" +
                @"stepSize: 100000" + "\r\n" +
                @"}," + "\r\n" +
                @"}, {" + "\r\n" +
                @"id: ""y-axis-2""," + "\r\n" +
                @"type: ""linear"", " + "\r\n" +
                @"position: ""right""," + "\r\n" +
                @"ticks:{" + "\r\n" +
                @"max:" + close.Max().ToString() + "," + "\r\n" +
                @"min: " + close.Min().ToString() + "," + "\r\n" +
                @"stepSize: 10000" + "\r\n" +
                @"}," + "\r\n" +
                @"gridLines:{" + "\r\n" +
                @"drawOnChartArea: true, " + "\r\n" +
                @"}," + "\r\n" +
                @"}]," + "\r\n" +
                @"}" + "\r\n" +
                @"};" + "\r\n" +
                @"</script>" + "\r\n" +
                @" </body>" + "\r\n" +
                @"</html>" + "\r\n"
                ) ;
            writer.Close();
            //System.Diagnostics.Process.Start(@"/Applications/Google Chrome.app", @"./line_chart.html");
            System.Diagnostics.Process.Start(@"/Applications/Google Chrome.app/Contents/MacOS/Google Chrome", @"./line_chart.html");
        }


        static private string GenerateNumericalLabel(List<double> data)
        {
            string label = "labels: [";
            var num_array = Enumerable.Range(0, data.Count).ToArray();
            label += string.Join(", ", num_array) + "],\r\n";
            return label;
        }

        static private string GenerateData(List<double> data)
        {
            string d = "data: [";
            d += string.Join(", ", data) + "],\r\n";
            return d;
        }


        static private string generateChart(string chart_type, int axis_id, string label, List<double> data, int[] color)
        {
            string res = @"{" + "\r\n" +
                @"type: '" + chart_type+"'," + "\r\n" +
                @"label: '" + label + "'," + "\r\n" +
                GenerateData(data) +
                @"borderColor : 'rgba(" + color[0] + "," + color[1] + "," + color[2] + ",0.8)'," + "\r\n" +
                //@"backgroundColor: 'rgba(" + color[0] + "," + color[1] + "," + color[2] + ",0.8)'," + "\r\n" +
                @"pointStyle: 'circle'," + "\r\n" +
                @"pointRadius: 0, " + "\r\n" +
                @"borderWidth: 1," + "\r\n" +
                @"yAxisID: 'y-axis-" + axis_id + "'," + "\r\n" +
                @"}," + "\r\n";
            return res;
        }

        static private string generatePlotChart(int axis_id, string label, List<double> data, int[] color)
        {
            string res = @"{" + "\r\n" +
                @"type: '" + "line" + "'," + "\r\n" +
                @"label: '" + label + "'," + "\r\n" +
                GenerateData(data) +
                @"borderColor : 'rgba(" + color[0] + "," + color[1] + "," + color[2] + ",0.8)'," + "\r\n" +
                @"backgroundColor: 'rgba(" + color[0] + "," + color[1] + "," + color[2] + ",0.8)'," + "\r\n" +
                @"borderWidth: 1," + "\r\n" +
                @"showLine: false," + "\r\n" +
                @"yAxisID: 'y-axis-" + axis_id + "'," + "\r\n" +
                @"}," + "\r\n";
            return res;
        }



        //単純なサンプリング
        static public List<double> timeseriesSamplingData(List<double> data, int sampling_freq)
        {
            var res = new List<double>();
            for(int i=0; i<data.Count; i++)
            {
                res.Add(data[i]);
                i += sampling_freq - 1;
            }
            return res;
        }

        //buy / sell entry pointsのサンプリング。
        static public List<double> buysellSamplingData(List<double> data, int sampling_freq)
        {
            var res = new List<double>();
            for (int i = 0; i < data.Count; i++)
            {
                if (data.GetRange(i, sampling_freq + i < data.Count ? sampling_freq:data.Count - i).Contains(1.0))
                    res.Add(MarketData.Close[i+sampling_freq]);
                else
                    res.Add(0.0);
                i += sampling_freq - 1;
            }
            return res;
        }

        //buy / sell entry pointsのサンプリング。
        static public List<double> buysellSamplingData2(List<double> data, int sampling_freq)
        {
            var res = new List<double>();
            for (int i = 0; i < data.Count; i++)
            {
                var d = 0.0;
                for(int j=0; j<sampling_freq; j++)
                {
                    if (data[i + j] > 0)
                        d = data[i + j];
                }
                res.Add(d);
                i += sampling_freq - 1;
            }
            return res;
        }

    }
}
