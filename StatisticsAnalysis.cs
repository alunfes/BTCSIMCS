using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;


namespace BTCSIM
{
    public class StatisticsAnalysis
    {
        public StatisticsAnalysis()
        {
        }


        /*
         * ・エントリー方法と利確・損切りタイミングの組み合わせとパフォーマンスの調査
         * side: buy, sell
         * エントリー方法：エントリー回数、エントリー間隔（時間、価格変動）、エントリーサイズ（残り資産のx%；xを関数で表現）
         * イグジット方法：利確・損切り率（0.1% - 5%, 0.1%幅）
         * 
         * Results:
         * parameter_data <analysis_id, [parameter_indexes]>
         * performance_data <analysis_id, [performance_data]>
         * 
         */
        public void startAnalysis1(List<int> data_entry_point, List<int> entry_num, List<int> entry_interval_miniute, List<int> entry_interval_price, List<double> ptlc_ratio)
        {
            var max_size = 10000.0;
            var buy_pl = new List<double>();
            var sell_pl = new List<double>();
            var buy_holding_period = new List<double>();
            var sell_holding_period = new List<double>();
            var buy_max_dd = new List<double>();
            var sell_max_dd = new List<double>();
            var buy_max_pl = new List<double>();
            var sell_max_pl = new List<double>();
            var buy_win_rate = new List<double>();
            var sell_win_rate = new List<double>();
            var pl_dic = new ConcurrentDictionary<int, double>();
            var period_dic = new ConcurrentDictionary<int, int>();
            var dd_dic = new ConcurrentDictionary<int, double>();
            var paremeter_data = new ConcurrentDictionary<int, int[]>();
            var option = new ParallelOptions();
            option.MaxDegreeOfParallelism = System.Environment.ProcessorCount;

            var parameter_id = 0;
            var num_all_trials = entry_num.Count * entry_interval_miniute.Count * ptlc_ratio.Count * ptlc_ratio.Count;
            for (int i = 0; i < entry_num.Count; i++)
            {
                for (int j = 0; j < entry_interval_miniute.Count; j++)
                {
                    for (int k = 0; k < ptlc_ratio.Count; k++) // for pt
                    {
                        for (int l = 0; l < ptlc_ratio.Count; l++) // for lc
                        {
                            var buy_performance_data = new ConcurrentDictionary<int, double[]>();
                            var sell_performance_data = new ConcurrentDictionary<int, double[]>();
                            /*
                            Parallel.For(0, data_entry_point.Count, option, dt_ind =>
                            {
                                buy_performance_data[dt_ind] = calc_analysis1(max_size, "buy", 0, data_entry_point[dt_ind], entry_num[i], entry_interval_miniute[j], ptlc_ratio[k], ptlc_ratio[l]);
                                sell_performance_data[dt_ind] = calc_analysis1(max_size, "sell", 0, data_entry_point[dt_ind], entry_num[i], entry_interval_miniute[j], ptlc_ratio[k], ptlc_ratio[l]);
                            });
                            */
                            for (int dt_ind = 0; dt_ind < data_entry_point.Count; dt_ind++)
                            {
                                buy_performance_data[dt_ind] = calc_analysis1(max_size, "buy", 0, data_entry_point[dt_ind], entry_num[i], entry_interval_miniute[j], ptlc_ratio[k], ptlc_ratio[l]);
                                sell_performance_data[dt_ind] = calc_analysis1(max_size, "sell", 0, data_entry_point[dt_ind], entry_num[i], entry_interval_miniute[j], ptlc_ratio[k], ptlc_ratio[l]);
                            }
                            paremeter_data[parameter_id] = new int[6] {Convert.ToInt32(max_size), 0, i, j, k, l };
                            var converted_buy_performance = convert_res(buy_performance_data);
                            var converted_sell_performance = convert_res(sell_performance_data);
                            buy_pl.Add(Math.Round(converted_buy_performance.Item1.Average(),6));
                            sell_pl.Add(Math.Round(converted_sell_performance.Item1.Average(),6));
                            buy_holding_period.Add(Math.Round(converted_buy_performance.Item2.Average(),1));
                            sell_holding_period.Add(Math.Round(converted_sell_performance.Item2.Average(),1));
                            buy_max_dd.Add(Math.Round(converted_buy_performance.Item3.Average(),6));
                            sell_max_dd.Add(Math.Round(converted_sell_performance.Item3.Average(),6));
                            buy_max_pl.Add(Math.Round(converted_buy_performance.Item4.Average(),6));
                            sell_max_pl.Add(Math.Round(converted_sell_performance.Item4.Average(),6));
                            buy_win_rate.Add(converted_buy_performance.Item5);
                            sell_win_rate.Add(converted_sell_performance.Item5);
                            if (parameter_id % 1000 == 0)
                            {
                                var progress_ratio = Math.Round(Convert.ToDouble(parameter_id) / num_all_trials, 4);
                                Console.WriteLine("************************************  (" + progress_ratio.ToString() + "%)" + parameter_id.ToString() + " / " + num_all_trials.ToString() + "  ************************************");
                                Console.WriteLine("id=" + parameter_id.ToString() + ", entry_num=" + entry_num[i].ToString() + ", interval=" + entry_interval_miniute[j].ToString() + ", pt ratio=" + ptlc_ratio[k].ToString() + ", lc ratio=" + ptlc_ratio[l].ToString());
                                Console.WriteLine("BUY: " +"holding period="+buy_holding_period.Last().ToString() +"win rate=" + converted_buy_performance.Item5.ToString() + ", pl=" + buy_pl.Last().ToString() + ", max_dd=" + buy_max_dd.Last().ToString() + ", max pl=" + buy_max_pl.Last());
                                Console.WriteLine("SEL: " + "holding period=" + sell_holding_period.Last().ToString() + "win rate=" + converted_sell_performance.Item5.ToString() + ", pl=" + sell_pl.Last().ToString() + ", max_dd=" + sell_max_dd.Last().ToString() + ", max pl=" + sell_max_pl.Last());
                            }
                            parameter_id += 1;
                        }
                    }
                }
            }
            /*
            for (int i = 0; i < entry_num.Count; i++)
            {
                for (int j = 0; j < entry_interval_price.Count; j++)
                {
                    for (int k = 0; k < ptlc_ratio.Count; k++)
                    {
                        for (int l = 0; l < ptlc_ratio.Count; l++) // for lc
                        {
                            Parallel.For(0, data_entry_point.Count, option, dt_ind =>
                            {
                                buy_paremeter_data[parameter_id] = new int[4] { 0, i, j, k };
                                buy_performance_data[parameter_id] = calc_analysis1(max_size, "buy", dt_ind, 1, i, j, k, l);
                                sell_paremeter_data[parameter_id] = new int[4] { 0, i, j, k };
                                sell_performance_data[parameter_id] = calc_analysis1(max_size, "sell", dt_ind, 1, i, j, k, l);
                                parameter_id += 1;
                            }
                        }
                    }
                }
            }*/
            Console.WriteLine("Writing results...");
            using (StreamWriter sw = new StreamWriter("statistics analysis1" + ".csv", false, Encoding.UTF8))
            {
                sw.WriteLine("ID,entry num,entry interval,pt,lc,buy holding period,sell holding period,buy pl,sell pl,buy win rate,sell win rate,buy max dd,sell max dd,buy max pl,sell max pl");
                for (int i=0; i<parameter_id; i++)
                {
                    sw.WriteLine(i.ToString()+","+entry_num[paremeter_data[i][2]].ToString()+","+ entry_interval_miniute[paremeter_data[i][3]].ToString()+","+
                        ptlc_ratio[paremeter_data[i][4]].ToString()+","+ ptlc_ratio[paremeter_data[i][5]].ToString()+","+buy_holding_period[i].ToString()+","+sell_holding_period[i].ToString()+","+buy_pl[i].ToString()+","+sell_pl[i].ToString()+","+
                        buy_win_rate[i].ToString()+","+sell_win_rate[i].ToString()+","+buy_max_dd[i].ToString()+","+sell_max_dd[i].ToString()+","+
                        buy_max_pl[i].ToString()+","+sell_max_pl[i].ToString());
                }
            }
            Console.WriteLine("Completed Statistics Analysis1");
        }


        private (List<double>, List<double>, List<double>, List<double>, double) convert_res(ConcurrentDictionary<int, double[]> pd)
        {
            var pls = new List<double>();
            var periods = new List<double>();
            var mdds = new List<double>();
            var mpls = new List<double>();
            var win_rate = 0.0;
            var num_win = 0;
            foreach (var d in pd.Values)
            {
                pls.Add(d[0]);
                periods.Add(d[1]);
                mdds.Add(d[2]);
                mpls.Add(d[3]);
                if (d[0] > 0)num_win++;
            }
            if (num_win > 0)
                win_rate = Convert.ToDouble(num_win) / pd.Values.Count;
            else
                win_rate = 0;
            return (pls, periods, mdds, mpls, Math.Round(win_rate,4));
        }

        private double[] calc_analysis1(double max_size, string side, int entry_interval_choice, int data_entry_point, int entry_num, int entry_interval, double pt_ratio, double lc_ratio)
        {
            var ave_price = 0.0;
            var total_size = 0.0;
            var num_current_entry = 1;
            var last_entry_ind = 0;
            var current_pl = 0.0;
            var max_dd = 0.0;
            var max_pl = 0.0;
            var current_ind = data_entry_point;
            var taker_fee = 0.00075;
            while (true)
            {
                if (MarketData.Close.Count > current_ind)
                {
                    if (num_current_entry <= entry_num && current_ind >= last_entry_ind + entry_interval)
                    {
                        var a = max_size / (Convert.ToDouble(1 + entry_num) * Convert.ToDouble(entry_num) / 2.0); //a = Max_size / Σ(num_entry), aはentry_num回entryしたときに合計のsizeが丁度max_sizeになる傾き
                        var y = a * num_current_entry; //yは今回のエントリーサイズ、回を追うごとにaだけ大きなサイズでエントリーする
                        var p = side == "buy" ? MarketData.Ask[current_ind] * (1.0+taker_fee) : MarketData.Bid[current_ind] * (1.0 - taker_fee);
                        ave_price = (ave_price * total_size + p * y) / (total_size + y);
                        total_size += y;
                        num_current_entry++;
                        last_entry_ind = current_ind;
                    }
                    //check pt / lc
                    //current_pl = side == "buy" ? (MarketData.Close[current_ind] - ave_price) / ave_price * total_size : (ave_price - MarketData.Close[current_ind]) / ave_price * total_size;
                    if (ave_price > 0)
                        current_pl = side == "buy" ? (MarketData.Bid[current_ind] - ave_price * (1.0 + taker_fee)) / ave_price : (ave_price * (1.0 - taker_fee) - MarketData.Ask[current_ind]) / ave_price;
                    else
                        current_pl = 0;
                    //var pl_ratio = current_pl / max_size;
                    var pl_ratio = current_pl * 100.0;
                    if (max_dd > pl_ratio)
                        max_dd = pl_ratio;
                    if (max_pl < pl_ratio)
                        max_pl = pl_ratio;

                    if (pl_ratio >= pt_ratio || pl_ratio <= -lc_ratio) //pt, lc基準にヒットした時に終了
                    {
                        return new double[] {pl_ratio, Convert.ToDouble(current_ind - last_entry_ind), max_dd, max_pl};
                    }

                    current_ind ++;
                }
                else //MarketDataの最後に到達したらその時点のperformanceデータを返す
                {
                    //current_pl = side == "buy" ? (MarketData.Close.Last() - ave_price) / ave_price * total_size : (ave_price - MarketData.Close.Last()) / ave_price * total_size;
                    if (ave_price > 0)
                        current_pl = side == "buy" ? (MarketData.Close.Last() - ave_price * (1.0 + taker_fee)) / ave_price : (ave_price * (1.0 - taker_fee) - MarketData.Close.Last()) / ave_price;
                    else
                        current_pl = 0;
                    //var pl_ratio = current_pl / max_size;
                    var pl_ratio = current_pl * 100.0;
                    if (max_dd > pl_ratio)
                        max_dd = pl_ratio;
                    if (max_pl < pl_ratio)
                        max_pl = pl_ratio;
                    return new double[] { pl_ratio, Convert.ToDouble(current_ind - last_entry_ind), max_dd, max_pl };
                }
            }

        }



    }
}
