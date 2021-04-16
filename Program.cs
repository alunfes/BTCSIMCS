﻿using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;


namespace BTCSIM
{
    class CombinedAC
    {
        public static (List<double>, int, double, double) calcCombinedAC(List<SimAccount> ac_list) //totla_pl_log, num_trade, win_rate, sharp ratio
        {
            var combined_total_pl = new List<double>();
            double combined_num_trade = 0;
            double combined_num_win = 0;
            double combined_win_rate = 0;
            double combined_sharp_ratio = 0;
            double current_pl = 0;
            for (int i = 0; i < ac_list.Count; i++)
            {
                for (int j = 0; j < ac_list[i].total_pl_list.Count; j++)
                {
                    combined_total_pl.Add(ac_list[i].total_pl_list[j] + current_pl);
                }
                current_pl = combined_total_pl.Last();
                combined_num_trade += ac_list[i].performance_data.num_trade;
                combined_num_win += ac_list[i].performance_data.num_win;
            }
            combined_win_rate = Math.Round(combined_num_win / combined_num_trade, 4);

            List<double> change = new List<double>();
            for (int i = 1; i < combined_total_pl.Count; i++)
            {
                if (combined_total_pl[i - 1] != 0)
                    change.Add((combined_total_pl[i] - combined_total_pl[i - 1]) / combined_total_pl[i - 1]);
                else
                    change.Add(0);
            }
            var doubleList = change.Select(a => Convert.ToDouble(a)).ToArray();

            //平均値算出
            double mean = doubleList.Average();
            //自乗和算出
            double sum2 = doubleList.Select(a => a * a).Sum();
            //分散 = 自乗和 / 要素数 - 平均値^2
            double variance = sum2 / Convert.ToDouble(doubleList.Length) - mean * mean;
            //標準偏差 = 分散の平方根
            var stdv = Math.Sqrt(variance);
            if (stdv != 0)
                combined_sharp_ratio = Math.Round(combined_total_pl.Last() / stdv, 4);
            else
                combined_sharp_ratio = 0;

            return (combined_total_pl, Convert.ToInt32(combined_num_trade), combined_win_rate, combined_sharp_ratio);
        }
    }


    class MainClass
    {
        private static void displaySimResult(SimAccount ac, string title)
        {
            Console.WriteLine("pl=" + ac.performance_data.total_pl);
            Console.WriteLine("pl ratio=" + ac.performance_data.total_pl_ratio);
            Console.WriteLine("num trade=" + ac.performance_data.num_trade);
            Console.WriteLine("num market order=" + ac.performance_data.num_maker_order);
            Console.WriteLine("win rate=" + ac.performance_data.win_rate);
            Console.WriteLine("sharp_ratio=" + ac.performance_data.sharp_ratio);
            Console.WriteLine("num_buy=" + ac.performance_data.num_buy);
            Console.WriteLine("num_sell=" + ac.performance_data.num_sell);
            Console.WriteLine("buy_pl=" + ac.performance_data.buy_pl_list.Sum());
            Console.WriteLine("sell_pl=" + ac.performance_data.sell_pl_list.Sum());
            Console.WriteLine("max_dd=" + ac.performance_data.max_dd);
            Console.WriteLine("max_pl=" + ac.performance_data.max_pl);
            Console.WriteLine("ave_holding_period=" + ac.holding_data.holding_period_list.Average());
            //LineChart.DisplayLineChart2(ac.performance_data.total_capital_list, ac.log_data.close_log, ac.log_data.buy_points.Values.ToList(), ac.log_data.sell_points.Values.ToList(), title);
            var table_labels = new List<string>() {"PL Ratio", "Num Trade", "Win Rate", "Max DD", "Max PL", "Ave Holding Period", "Num Force Exit"};
            var table_data = new List<string>() {Math.Round(ac.performance_data.total_pl_ratio,4).ToString(), ac.performance_data.num_trade.ToString(), Math.Round(ac.performance_data.win_rate,4).ToString(), Math.Round(ac.performance_data.max_dd,4).ToString(),
            Math.Round(ac.performance_data.max_pl,4).ToString(), Math.Round(ac.holding_data.holding_period_list.Average(),1).ToString(), ac.performance_data.num_force_exit.ToString()};
            LineChart.DisplayLineChart3(ac.performance_data.total_capital_list, ac.log_data.close_log, ac.performance_data.num_trade_list, table_labels, table_data, "from="+ac.start_ind.ToString()+", to="+ac.end_ind.ToString());
            System.Diagnostics.Process.Start(@"/Applications/Google Chrome.app/Contents/MacOS/Google Chrome", @"./line_chart.html");
        }

        private static SimAccount doSim(int from, int to, int max_amount, int sim_type, int best_island_id, bool display_chart, double nn_threshold)
        {
            Console.WriteLine("Started Read Weight SIM");
            var ga = new GA(0);
            var chromo = ga.readWeights(best_island_id, false);
            if (sim_type == 0)
                return ga.sim_ga_limit(from, to, max_amount, chromo, from.ToString() + " - " + to.ToString() + ", dt:" + MarketData.Dt[from].ToString() + " - " + MarketData.Dt[to - 1] + ", Best Island=" + best_island_id.ToString(), display_chart);
            else
                return ga.sim_ga_market_limit(from, to, max_amount, chromo, from.ToString() + " - " + to.ToString() + ", dt:" + MarketData.Dt[from].ToString() + " - " + MarketData.Dt[to - 1] + ", Best Island=" + best_island_id.ToString(), display_chart, nn_threshold);
        }

        private static int doGA(int from, int to, int max_amount, int num_island, int num_chromo, int num_generation, int banned_move_period, int[] units, double mutation_rate, double move_ratio, int[] index, bool display_chart, double nn_threshold)
        {
            Console.WriteLine("Started Island GA SIM");
            RandomSeed.initialize();
            var ga_island = new GAIsland();
            ga_island.start_ga_island(from, to, max_amount, num_island, banned_move_period, move_ratio, num_chromo, num_generation, units, mutation_rate, 1, nn_threshold, index);
            return ga_island.best_island;
        }

        private static int doWinGA(int from, int to, int num_random_windows, int num_island, int num_chromo, int num_generation, int banned_move_period, int[] units, double mutation_rate, double move_ratio, int[] index, bool display_chart, double nn_threshold)
        {
            Console.WriteLine("Started Island Win GA SIM");
            RandomSeed.initialize();
            var ga_island = new GAIsland();
            ga_island.start_win_ga_island(from, to, num_random_windows, num_island, banned_move_period, move_ratio, num_chromo, num_generation, units, mutation_rate, nn_threshold, index);
            return ga_island.best_island;
        }

        private static SimAccount doWinSim(int from, int to, int best_island_id, bool display_chart, double nn_threshold)
        {
            Console.WriteLine("Started Read Weight Win SIM");
            var ga = new GA(0);
            var chromo = ga.readWeights(best_island_id, false);
            var sim = new Sim();
            var ac = new SimAccount();
            ac = sim.sim_win_ga_market(from, to, chromo, ac, nn_threshold);

            Console.WriteLine("pl=" + ac.performance_data.total_pl);
            Console.WriteLine("pl ratio=" + ac.performance_data.total_pl_ratio);
            Console.WriteLine("num trade=" + ac.performance_data.num_trade);
            Console.WriteLine("num market order=" + ac.performance_data.num_maker_order);
            Console.WriteLine("win rate=" + ac.performance_data.win_rate);
            Console.WriteLine("sharp_ratio=" + ac.performance_data.sharp_ratio);
            Console.WriteLine("num_buy=" + ac.performance_data.num_buy);
            Console.WriteLine("num_sell=" + ac.performance_data.num_sell);
            Console.WriteLine("buy_pl=" + ac.performance_data.buy_pl_list.Sum());
            Console.WriteLine("sell_pl=" + ac.performance_data.sell_pl_list.Sum());
            if (display_chart)
                LineChart.DisplayLineChart(ac.total_pl_list, "from=" + from.ToString() + ", to=" + to.ToString() + ", pl=" + ac.performance_data.total_pl.ToString() + ", num_trade=" + ac.performance_data.num_trade.ToString());
            return ac;

        }

        private static SimAccount doMultiSim(int from, int to, int max_amount, List<int> best_chrom_log_id, bool display_chart, List<double> nn_threshold)
        {
            Console.WriteLine("Started Multi SIM");
            var chromos = new Gene2[best_chrom_log_id.Count];
            var ga = new GA(0);
            for (int i = 0; i < best_chrom_log_id.Count; i++)
            {
                chromos[i] = ga.readWeights(best_chrom_log_id[i], true);
            }
            var title = "Combined PL Ratio - " + from.ToString() + " - " + to.ToString() + ", dt:" + MarketData.Dt[from].ToString() + " - " + MarketData.Dt[to - 1];
            return ga.sim_ga_multi_chromo(from, to, max_amount, chromos.ToList(), title, display_chart, nn_threshold);
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("# of CPU cores=" + System.Environment.ProcessorCount.ToString());

            var key = "";
            while (true)
            {
                Console.WriteLine("\"ga\" : island GA");
                Console.WriteLine("\"sim\" : read sim");
                Console.WriteLine("\"mul ga\" : multi strategy ga");
                Console.WriteLine("\"mul sim\" : multi strategy sim");
                Console.WriteLine("\"conti\" : do ga / sim continuously");
                Console.WriteLine("\"win ga\" : do win ga");
                Console.WriteLine("\"win sim\" : do win sim");
                Console.WriteLine("\"sa1\" : statistics analysis1");
                Console.WriteLine("\"sa2\" : statistics analysis2");
                Console.WriteLine("\"ptlc sim\" : ptlc periodical entry sim");
                Console.WriteLine("\"write\" : write MarketData");
                Console.WriteLine("\"test\" : test");
                key = Console.ReadLine();
                if (key == "ga" || key == "sim" || key == "mul ga" || key == "mul sim" || key == "win ga" || key == "conti" || key == "win ga" || key == "win sim" || key == "write" || key == "test" || key == "sa1" || key == "sa2" || key == "ptlc sim")
                    break;
            }


            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            Console.WriteLine("started program.");
            List<int> terms = new List<int>();
            for (int i = 10; i < 100; i = i + 10) { terms.Add(i); }
            MarketData.initializer(terms);

            var from = 1000;
            var to = MarketData.Close.Count-1;
            int max_amount = 1;
            var index = new int[] { 0, 0, 0, 1, 1, 0, 0 };
            double nn_threshold = 0.7;
            int best_island_id = 0;
            bool display_chart = true;
            var sim_type = 1; //0:limit, 1:market/limit


            if (key == "test")
            {
                var ac = doSim(from, to, max_amount, sim_type, best_island_id, display_chart, nn_threshold);
                LineChart.DisplayLineChart2(ac.total_pl_list, ac.log_data.close_log, ac.log_data.buy_points.Values.ToList(), ac.log_data.sell_points.Values.ToList(), "test");
                System.Diagnostics.Process.Start(@"/Applications/Google Chrome.app/Contents/MacOS/Google Chrome", @"./line_chart.html");
            }


            //read weight sim
            if (key == "sim")
            {
                var ac = doSim(from, to, max_amount, sim_type, best_island_id, display_chart, nn_threshold);
                LineChart.DisplayLineChart2(ac.total_pl_list, ac.log_data.close_log, ac.log_data.buy_points.Values.ToList(), ac.log_data.sell_points.Values.ToList(), "test");
                System.Diagnostics.Process.Start(@"/Applications/Google Chrome.app/Contents/MacOS/Google Chrome", @"./line_chart.html");
            }
            //island ga
            else if (key == "ga")
            {
                int num_island = 1;
                int num_chromos = 4;
                int num_generations = 100;
                int banned_move_period = 5;
                var units = new int[] { 42, 90, 5 };
                var mutation_rate = 0.7;
                var move_ratio = 0.5;
                best_island_id = doGA(from, to, max_amount, num_island, num_chromos, num_generations, banned_move_period, units, mutation_rate, move_ratio, index, display_chart, nn_threshold);
                doSim(from, to, max_amount, sim_type, best_island_id, display_chart, nn_threshold);
                doSim(to, MarketData.Close.Count - 1, max_amount, sim_type, best_island_id, display_chart, nn_threshold);
            }
            //multi strategy combination sim
            else if (key == "mul ga")
            {
                var index_list = new List<int[]> { new int[] { 1, 0, 0, 0, 0, 0, 0 }, new int[] { 0, 1, 0, 0, 0, 0, 0 }, new int[] { 0, 0, 1, 0, 0, 0, 0 } };
                var units_list = new List<int[]> { new int[] { 34, 10, 5 }, new int[] { 34, 10, 5 }, new int[] { 34, 10, 5 } };
                var best_pl_list = new List<List<double>>();
                var best_ac_list = new List<SimAccount>();
                int num_island = 2;
                int num_chromos = 4;
                int num_generations = 30;
                int banned_move_period = 3;
                var mutation_rate = 0.7;
                var move_ratio = 0.5;
                var id_list = new List<int>();
                var nn_threshold_list = new List<double>();
                for (int i = 0; i < index_list.Count; i++)
                {
                    best_island_id = doGA(from, to, max_amount, num_island, num_chromos, num_generations, banned_move_period, units_list[i], mutation_rate, move_ratio, index_list[i], display_chart, nn_threshold);
                    var ac = doSim(from, to, max_amount, sim_type, best_island_id, display_chart, nn_threshold);
                    best_pl_list.Add(ac.total_pl_ratio_list);
                    best_ac_list.Add(ac);
                    if (File.Exists(@"./log_best_weight_ID-" + i.ToString() + ".csv"))
                        File.Delete(@"./log_best_weight_ID-" + i.ToString() + ".csv");
                    File.Copy(@"./best_weight_ID-" + best_island_id.ToString() + ".csv", @"./log_best_weight_ID-" + i.ToString() + ".csv");
                    id_list.Add(i);
                    nn_threshold_list.Add(nn_threshold);
                }
                doMultiSim(from, to, max_amount, id_list, true, nn_threshold_list);
                doMultiSim(to, MarketData.Close.Count - 1, max_amount, id_list, true, nn_threshold_list);

            }
            else if (key == "mul sim")
            {
                var num_best_chromo = 3;
                var id_list = new List<int>();
                var nn_threshold_list = new List<double>();
                for (int i = 0; i < num_best_chromo; i++)
                {
                    id_list.Add(i);
                    nn_threshold_list.Add(nn_threshold);
                }
                doMultiSim(from, to, max_amount, id_list, true, nn_threshold_list);
            }
            else if (key == "win ga")
            {
                int num_island = 2;
                int num_chromos = 4;
                int num_generations = 200;
                int banned_move_period = 5;
                int num_random_windows = 10;
                index = new int[] { 0, 0, 0, 1, 0, 0, 0 };
                var units = new int[] { 10, 30, 3 };
                var mutation_rate = 0.5;
                var move_ratio = 0.2;

                best_island_id = doWinGA(from, to, num_random_windows, num_island, num_chromos, num_generations, banned_move_period, units, mutation_rate, move_ratio, index, display_chart, nn_threshold);
                doWinSim(from, to, best_island_id, true, nn_threshold);
                doWinSim(to, MarketData.Close.Count - 1, best_island_id, true, nn_threshold);
            }
            else if (key == "win sim")
            {
                doWinSim(from, to, best_island_id, true, nn_threshold);
            }
            else if(key == "sa1")
            {
                var sa = new StatisticsAnalysis();
                var data_entry_points = Enumerable.Range(0, MarketData.Close.Count).Where(i => i % 1000 == 0).ToList();
                //var entry_num = Enumerable.Range(1, 100).Where(i => i % 5 == 0).ToList();
                var entry_num = new List<int>() { 1,5,50,100};
                //var entry_interval = Enumerable.Range(1, 100).Where(i => i % 5 == 0).ToList();
                var entry_interval = new List<int>() {1,5,50,100 };
                //var ptlc_ratio = Enumerable.Range(2, 100).ToList().ConvertAll(x => Math.Round(x * 0.1,1)).ToList();
                var ptlc_ratio = new List<double>() {0.2, 1, 5, 15 };
                sa.startAnalysis1(data_entry_points, entry_num, entry_interval, entry_interval, ptlc_ratio);
            }
            else if (key == "sa2")
            {
                var sa = new StatisticsAnalysis();
                /*var leverages = new List<double>() { 0.5,1,1.5,2,2.5,3,5};
                var entry_num = new List<int>() { 1, 5, 50, 100 };
                var entry_interval = new List<int>() { 1, 5, 50, 100 };
                var pt_ratio = new List<double>() { 0.05, 0.07, 0.1, 0.15, 0.2, 0.25 };
                var lc_ratio = new List<double>() { -0.05, -0.07, -0.1, -0.15, -0.2, -0.25 };*/
                var leverages = new List<double>() { 0.5, 5 };
                var entry_num = new List<int>() { 1, 5};
                var entry_interval = new List<int>() { 1, 5};
                var pt_ratio = new List<double>() { 0.05, 0.15 };
                var lc_ratio = new List<double>() { -0.05, -0.15 };
                sa.startAnalysis2(pt_ratio, lc_ratio, leverages, entry_interval, entry_num);
            }
            else if (key == "conti")
            {
                int num_island = 2;
                int num_chromos = 4;
                int num_generations = 20;
                int banned_move_period = 2;
                var units = new int[] { 67, 5, 5, 5, 5 };
                var mutation_rate = 0.5;
                var move_ratio = 0.2;
                var sim_period = 5000;
                var ga_period = 10000;
                var conti_from = from;
                var ac_list = new List<SimAccount>();
                var all_pl_list = new List<double>();
                all_pl_list.Add(0);
                var all_num_trade = 0;

                while (to > sim_period + ga_period + conti_from)
                {
                    best_island_id = doGA(conti_from, conti_from + ga_period, max_amount, num_island, num_chromos, num_generations, banned_move_period, units, mutation_rate, move_ratio, index, display_chart, nn_threshold);
                    ac_list.Add(doSim(conti_from + ga_period, conti_from + ga_period + sim_period, max_amount, sim_type, best_island_id, true, nn_threshold));
                    foreach (var p in ac_list.Last().total_pl_list)
                        all_pl_list.Add(all_pl_list.Last() + p);
                    all_num_trade += ac_list.Last().performance_data.num_trade;
                    conti_from += sim_period;
                }
                Console.WriteLine("Total pl =" + all_pl_list.Last().ToString() + ", num trade=" + all_num_trade.ToString());
                LineChart.DisplayLineChart(all_pl_list, "from=" + (from + ga_period).ToString() + ", to=" + (conti_from + ga_period + sim_period).ToString() + ", Total pl =" + all_pl_list.Last().ToString() + ", num trade=" + all_num_trade.ToString());
            }
            else if(key=="ptlc sim")
            {
                var pt = 0.25;
                var lc = -0.2;
                var leverage = 3;
                var side = "buy";
                var entry_interval_minutes = 5;
                var entry_num = 100;
                var ac = new SimAccount();
                var sim = new Sim();
                ac = sim.sim_entry_timing_ptlc(from, to, ac, side, leverage, entry_interval_minutes, entry_num, pt, lc);
                displaySimResult(ac, "ptlc sim from:"+from.ToString() + " to:"+to.ToString()+ ", pl="+ac.performance_data.total_pl.ToString() + ", num="+ac.performance_data.num_trade.ToString());
            }
            else if (key == "write")
            {
                MarketData.writeData();
            }

            stopWatch.Stop();
            Console.WriteLine("Completed all processes.");
            Console.WriteLine("Time Elapsed (sec)=" + stopWatch.Elapsed.TotalSeconds.ToString());
        }
    }
}
