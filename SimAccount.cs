﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Linq;
using System.IO;



namespace BTCSIM
{

    public class PerformanceData
    {
        public double total_pl { get; set; }
        public double realized_pl { get; set; }
        public double total_capital { get; set; }
        public double initial_capital { get; set; }
        public List<double> realized_pl_list { get; set; }
        public List<double> total_capital_list { get; set; }
        public List<double> buy_pl_list { get; set; }
        public List<double> sell_pl_list { get; set; }
        public double unrealized_pl { get; set; }
        public List<double> unrealized_pl_list { get; set; } //record unrealided pl during holding period for NN input data
        public double unrealized_pl_ratio { get; set; }
        public List<double> unrealized_pl_ratio_list { get; set; }
        public double total_pl_ratio { get; set; }
        public List<double> buy_pl_ratio_list { get; set; }
        public List<double> sell_pl_ratio_list { get; set; }
        public double max_dd { get; set; }
        public double max_pl { get; set; }
        public int num_force_exit { get; set; }

        public int num_trade { get; set; }
        public List<int> num_trade_list { get; set; }
        public int num_buy { get; set; }
        public int num_sell { get; set; }
        public int num_maker_order { get; set; }
        public int num_win { get; set; }
        public double win_rate { get; set; }
        public double total_fee { get; set; }
        public double sharp_ratio { get; set; }

        public PerformanceData()
        {
            total_pl = 0.0;
            total_pl_ratio = 0.0;
            total_capital = 100000.0;
            initial_capital = 100000.0;
            realized_pl = 0.0;
            realized_pl_list = new List<double>();
            total_capital_list = new List<double>();
            buy_pl_list = new List<double>();
            sell_pl_list = new List<double>();
            buy_pl_ratio_list = new List<double>();
            sell_pl_ratio_list = new List<double>();
            unrealized_pl = 0.0;
            unrealized_pl_list = new List<double>();
            unrealized_pl_ratio = 0.0; //
            unrealized_pl_ratio_list = new List<double>();
            num_trade = 0;
            num_trade_list = new List<int>();
            num_buy = 0;
            num_sell = 0;
            win_rate = 0;
            num_win = 0;
            num_maker_order = 0;
            total_fee = 0.0;
            sharp_ratio = 0.0;
            max_dd = 0.0;
            max_pl = 0.0;
            num_force_exit = 0;
        }
    }

    public class OrderData
    {
        public int order_serial_num { get; set; } //active order serial num list
        public List<int> order_serial_list { get; set; } //latest order serial num
        public Dictionary<int, string> order_side { get; set; }
        public Dictionary<int, double> order_size { get; set; }
        public Dictionary<int, double> order_price { get; set; }
        public Dictionary<int, string> order_type { get; set; }
        public Dictionary<int, int> order_i { get; set; }
        public Dictionary<int, string> order_dt { get; set; }
        public Dictionary<int, Boolean> order_cancel { get; set; }
        public Dictionary<int, string> order_message { get; set; } //entry, pt, exit&entry

        public OrderData()
        {
            order_serial_num = -1;
            order_serial_list = new List<int>();
            order_side = new Dictionary<int, string>();
            order_size = new Dictionary<int, double>();
            order_price = new Dictionary<int, double>();
            order_type = new Dictionary<int, string>();
            order_i = new Dictionary<int, int>();
            order_dt = new Dictionary<int, string>();
            order_cancel = new Dictionary<int, bool>();
            order_message = new Dictionary<int, string>();
        }

        public string getLastOrderSide()
        {
            if (order_serial_list.Count > 0)
                return order_side[order_serial_list.Last()];
            else
                return "";
        }
        public double getLastOrderSize()
        {
            if (order_serial_list.Count > 0)
                return order_size[order_serial_list.Last()];
            else
                return 0;
        }
        public double getLastOrderPrice()
        {
            if (order_serial_list.Count > 0)
                return order_price[order_serial_list.Last()];
            else
                return 0;
        }
        public int getNumOrders()
        {
            if (order_serial_list.Count > 0)
                return order_serial_list.Count;
            else
                return 0;
        }
        public int getLastSerialNum()
        {
            if (order_serial_list.Count() > 0)
                return order_serial_list.Last();
            else
                return -1;
        }
    }

    public class HoldingData
    {
        public string holding_side { get; set; }
        public double holding_price { get; set; }
        public double holding_size { get; set; }
        public double holding_volume { get; set; }
        public int holding_entry_num { get; set; }
        public int holding_i { get; set; }
        public int holding_period { get; set; }
        public int holding_initial_i { get; set; }
        public List<int> holding_period_list { get; set; }

        public HoldingData()
        {
            holding_i = -1;
            holding_period = 0;
            holding_price = 0;
            holding_size = 0;
            holding_volume = 0;
            holding_entry_num = 0;
            holding_side = "";
            holding_period_list = new List<int>();
        }

        public void initialize_holding()
        {
            holding_i = -1;
            holding_period = 0;
            holding_price = 0;
            holding_size = 0;
            holding_volume = 0;
            holding_entry_num = 0;
            holding_side = "";
        }


        public void update_holding(string side, double price, double size, int i)
        {
            if (holding_side == "") //New Entry
            {
                holding_side = side;
                holding_price = price;
                holding_size = size;
                holding_volume = size * price;
                holding_entry_num++;
                holding_i = i;
                holding_period = 0;
                holding_initial_i = i;
            }
            else if(holding_side != side) //Opposit Entry
            {
                holding_period_list.Add(holding_period);
                holding_side = side;
                holding_price = price;
                holding_size = size;
                holding_volume = size * price;
                holding_i = i;
                holding_period = 0;
                holding_initial_i = i;
            }
            else if(holding_side == side) //Additional Entry
            {
                holding_side = side;
                holding_price = price;
                holding_size = size;
                holding_volume = size * price;
                holding_entry_num++;
                holding_i = i;
            }
        }
    }

    public class LogData
    {
        public DataSet log_data_set { get; set; }
        public DataTable log_data_table { get; set; }
        public List<double> total_pl_log { get; set; }
        public List<double> total_pl_ratio { get; set; }
        public List<double> close_log { get; set; }
        public Dictionary<int,double> buy_points { get; set; }
        public Dictionary<int, double> sell_points { get; set; }


        public LogData()
        {
            log_data_set = new DataSet();
            log_data_table = new DataTable("LodData");
            log_data_table.Columns.Add("total_pl", typeof(double));
            log_data_table.Columns.Add("total_fee", typeof(double));
            log_data_table.Columns.Add("dt", typeof(string));
            log_data_table.Columns.Add("i", typeof(Int64));
            log_data_table.Columns.Add("order_side", typeof(string));
            log_data_table.Columns.Add("order_type", typeof(string));
            log_data_table.Columns.Add("order_size", typeof(double));
            log_data_table.Columns.Add("order_price", typeof(double));
            log_data_table.Columns.Add("order_message", typeof(string));
            log_data_table.Columns.Add("holding_side", typeof(string));
            log_data_table.Columns.Add("holding_price", typeof(double));
            log_data_table.Columns.Add("holding_size", typeof(double));
            log_data_table.Columns.Add("action", typeof(string));
            log_data_set.Tables.Add(log_data_table);
            total_pl_log = new List<double>();
            total_pl_ratio = new List<double>();
            close_log = new List<double>();
            buy_points = new Dictionary<int, double>();
            sell_points = new Dictionary<int, double>();
        }

        public void add_log_data(int i, string dt, string action, HoldingData hd, OrderData od, PerformanceData pd)
        {
            total_pl_log.Add(pd.total_pl);
            if (od.order_serial_list.Count > 0)
            {
                log_data_table.Rows.Add(pd.total_pl, pd.total_fee, dt, i, od.order_side[od.order_serial_list.Last()], od.order_type[od.order_serial_list.Last()],
                    od.order_size[od.order_serial_list.Last()], od.order_price[od.order_serial_list.Last()], od.order_message[od.order_serial_list.Last()], hd.holding_side, hd.holding_price, hd.holding_size,
                    action);
            }
            else
            {
                log_data_table.Rows.Add(pd.total_pl, pd.total_fee, dt, i, "", "", 0, 0, "", hd.holding_side, hd.holding_price, hd.holding_size, action);
            }
        }
    }

    /*
     * iのohlcで判断して、i+1のopenでentry、i+1のohlcで約定判定
     * （キャンセルは即時反映、updateも即時反映）
     * 
     * conti simなどでもiは継続した値を使用しないといけない。
     * 
     * total_capital = initial_capital + total_pl
     * total_pl = realized_pl + unrealized_pl + total_fee
     * realized_pl = (exec_price - entry_price) * size
     * unrealized_pl = (close - entry_price) * size
     * fee = fee_rate * exec_price * size
     * 
     */
    public class SimAccount
    {
        public PerformanceData performance_data;
        public OrderData order_data;
        public HoldingData holding_data;
        public LogData log_data;

        public const double taker_fee = 0.00075;
        public const double maker_fee = -0.00025;
        public const double leverage_limit = 25;
        public const double required_margin_maintenace_rate = 0.8;
        public const double min_capital = 10000; //minimam required capital to allow place order
        public double margin_required = 0.0;
        public double margin_maintenace_rate = 0.0;
        public double leverage = 0.0;


        public int start_ind = 0;
        public int end_ind = 0;

        

        public List<double> total_pl_list = new List<double>();
        public List<double> total_pl_ratio_list = new List<double>();
        public List<double> total_fund_list = new List<double>();

        public SimAccount()
        {
            log_data = new LogData();
            performance_data = new PerformanceData();
            order_data = new OrderData();
            holding_data = new HoldingData();
            total_pl_list = new List<double>();
            total_pl_ratio_list = new List<double>();
            margin_required = 0.0;
            leverage = 0.0;
            start_ind = 0;
            end_ind = 0;
        }

        /*should be called after all sim calc*/
        public void calc_sharp_ratio()
        {
            List<double> change = new List<double>();
            for (int i = 1; i < log_data.total_pl_log.Count; i++)
            {
                if (log_data.total_pl_log[i - 1] != 0)
                    change.Add((log_data.total_pl_log[i] - log_data.total_pl_log[i - 1]) / log_data.total_pl_log[i - 1]);
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
                performance_data.sharp_ratio = performance_data.total_pl / stdv;
            else
                performance_data.sharp_ratio = 0;
        }


        /*
         * * total_capital = initial_capital + total_pl
         * total_pl = realized_pl + unrealized_pl + total_fee
         * realized_pl = (exec_price - entry_price) * size
         * unrealized_pl = (close - entry_price) * size
         * fee = fee_rate * exec_price * size
         */
        private void calc_performance_data(double close)
        {
            if (holding_data.holding_side != "")
            {
                var price_change_ratio = holding_data.holding_side == "buy" ? (close - holding_data.holding_price) / holding_data.holding_price : (holding_data.holding_price - close) / holding_data.holding_price;
                performance_data.unrealized_pl = holding_data.holding_side == "buy" ? (close - holding_data.holding_price) * holding_data.holding_size : (holding_data.holding_price - close) * holding_data.holding_size;
                performance_data.total_pl = performance_data.realized_pl + performance_data.unrealized_pl - performance_data.total_fee;
                performance_data.total_capital = performance_data.total_pl + performance_data.initial_capital;                
            }
            else
            {
                performance_data.unrealized_pl = 0.0;
                performance_data.unrealized_pl_ratio = 0.0;
                performance_data.total_pl = performance_data.realized_pl + performance_data.unrealized_pl - performance_data.total_fee;
                performance_data.total_capital = performance_data.total_pl + performance_data.initial_capital;
                //performance_data.unrealized_pl_list = new List<double>();
            }
            performance_data.unrealized_pl_ratio = performance_data.unrealized_pl != 0 ? performance_data.unrealized_pl / (performance_data.total_capital - performance_data.unrealized_pl) : 0;
            performance_data.total_pl_ratio = (performance_data.total_capital - performance_data.initial_capital) / performance_data.initial_capital;
            performance_data.unrealized_pl_list.Add(performance_data.unrealized_pl);
            performance_data.total_capital_list.Add(performance_data.total_capital);
            performance_data.unrealized_pl_ratio_list.Add(performance_data.unrealized_pl_ratio);
            total_pl_list.Add(performance_data.total_pl);
            total_pl_ratio_list.Add(performance_data.total_pl_ratio);

            if (performance_data.unrealized_pl_ratio < -0.3)
                Console.WriteLine("high unrealized pl ratio!");
            performance_data.num_trade_list.Add(performance_data.num_trade);
        }


        private void calc_margin_data(int i, double close)
        {
            if (holding_data.holding_side != "")
            {
                margin_required = Math.Round(holding_data.holding_volume / leverage_limit, 2);
                margin_maintenace_rate = Math.Round(performance_data.total_capital / margin_required, 4);
                leverage = Math.Round(holding_data.holding_volume / performance_data.total_capital,4);
                if (margin_maintenace_rate <= required_margin_maintenace_rate)
                {
                    Console.WriteLine("Maintenace Margin is too small, force close all positions!");
                    Console.WriteLine("Margin Rate="+margin_maintenace_rate+", leverage="+leverage);
                    performance_data.num_force_exit++;
                    exit_all(i, MarketData.Dt[i].ToString());
                }   
            }
            else
            {
                leverage = 0.0;
                margin_required = 0.0;
                margin_maintenace_rate = 0.0;
            }
        }
        
        public void move_to_next(int i, string dt, double open, double high, double low, double close)
        {
            if (start_ind <= 0)
                start_ind = i;
            end_ind = i;
            check_cancel(i, dt);
            check_execution(i, dt, open, high, low);
            holding_data.holding_period = holding_data.holding_initial_i > 0 ? i - holding_data.holding_initial_i : 0;
            holding_data.holding_volume = holding_data.holding_side != "" ? holding_data.holding_size * close : 0;
            calc_performance_data(close);
            calc_margin_data(i, close);
            if (performance_data.num_trade > 0)
                performance_data.win_rate = Math.Round(Convert.ToDouble(performance_data.num_win) / Convert.ToDouble(performance_data.num_trade), 4);
            log_data.add_log_data(i, dt, "move to next", holding_data, order_data, performance_data);
            
            log_data.close_log.Add(MarketData.Close[i]);
            if (log_data.buy_points.Keys.Contains(i) == false)
                log_data.buy_points[i] = 0;
            if (log_data.sell_points.Keys.Contains(i) == false)
                log_data.sell_points[i] = 0;

            //Console.WriteLine("unrealized pl=" + performance_data.unrealized_pl + ", maring rate=" + margin_maintenace_rate + ", lev=" + leverage + ", size=" + holding_data.holding_size + ", price=" + holding_data.holding_price + ", capital=" + performance_data.total_capital + ", close=" + close);
        }

        //close all holding positions and calc pl
        public void last_day(int i, double close)
        {
            order_data = new OrderData();
            if (holding_data.holding_side != "")
            {
                calc_executed_pl(close, holding_data.holding_size, i);
                performance_data.num_trade++;
                holding_data.holding_period_list.Add(holding_data.holding_period);
                holding_data.initialize_holding();
                performance_data.unrealized_pl = 0;
            }
            calc_performance_data(close);
            calc_margin_data(i, close);
            performance_data.max_dd = Math.Round(performance_data.unrealized_pl_ratio_list.Min(),6);
            performance_data.max_pl = Math.Round(performance_data.unrealized_pl_ratio_list.Max(),6);
            if (performance_data.num_trade > 0)
                performance_data.win_rate = Math.Round(Convert.ToDouble(performance_data.num_win) / Convert.ToDouble(performance_data.num_trade), 4);
            log_data.close_log.Add(MarketData.Close[i]);
            if (log_data.buy_points.Keys.Contains(i) == false)
                log_data.buy_points[i] = 0;
            if (log_data.sell_points.Keys.Contains(i) == false)
                log_data.sell_points[i] = 0;
        }

        public void entry_order(string type, string side, double size, double price, int i, string dt, string message)
        {
            var flg_check_basics = false;
            var flg_check_leverage = false;
            var flg_check_min_capital = false;
            if (size > 0 && (side == "buy" || side == "sell"))
                flg_check_basics = true;
            if (side != holding_data.holding_side && holding_data.holding_side != "" || ((MarketData.Close[i] * size) + holding_data.holding_volume) / performance_data.total_capital < leverage_limit)
                flg_check_leverage = true;
            if (performance_data.total_capital > min_capital)
                flg_check_min_capital = true;

            //allow order entry only when all flg cleared or opposite side entry (losscut and etc)
            if (flg_check_basics && flg_check_leverage && flg_check_min_capital || (flg_check_basics && side != holding_data.holding_side && holding_data.holding_side!=""))
            {
                order_data.order_serial_num++;
                order_data.order_serial_list.Add(order_data.order_serial_num);
                order_data.order_type[order_data.order_serial_num] = type;
                order_data.order_side[order_data.order_serial_num] = side;
                order_data.order_size[order_data.order_serial_num] = size;
                order_data.order_price[order_data.order_serial_num] = price;
                order_data.order_i[order_data.order_serial_num] = i;
                order_data.order_dt[order_data.order_serial_num] = dt;
                order_data.order_cancel[order_data.order_serial_num] = false;
                order_data.order_message[order_data.order_serial_num] = message;
                log_data.add_log_data(i, dt, "entry order " + side + "-" + type, holding_data, order_data, performance_data);
            }
            else
            {
                if (flg_check_basics == false)
                    Console.WriteLine("Entry Order failed due to basic check !");
                if (flg_check_leverage == false)
                    Console.WriteLine("Entry order failed due to over leverage !");
                if (flg_check_min_capital == false)
                {
                    //Console.WriteLine("Entry order failed due to too small capital !");
                }
            }
        }

        public void update_order_price(double update_price, int order_serial_num, int i, string dt)
        {
            if (order_data.getLastOrderSide() == "buy" && order_data.getLastOrderPrice() > update_price) { }
            //Console.WriteLine(i.ToString()+": buy update issue:"+order_data.getLastOrderPrice().ToString() + " -> "+update_price.ToString());
            else if (order_data.getLastOrderSide() == "sell" && order_data.getLastOrderPrice() < update_price) { }
            //Console.WriteLine(i.ToString() + ": sell update issue:" + order_data.getLastOrderPrice().ToString() + " -> " + update_price.ToString());

            if (update_price > 0 && order_data.order_serial_list.Contains(order_serial_num))
            {
                order_data.order_price[order_serial_num] = update_price;
                //order_data.order_i[order_serial_num] = i;
                //order_data.order_message[order_serial_num] = "updated-" + order_data.order_message[order_serial_num];
                log_data.add_log_data(i, dt, "update order price", holding_data, order_data, performance_data);
            }
            else
            {
                Console.WriteLine("invalid update price or order_serial_num in update_order_price !");
            }
        }

        public void update_order_amount(double update_amount, int order_serial_num, int i, string dt)
        {
            if (update_amount > 0 && order_data.order_serial_list.Contains(order_serial_num))
            {
                order_data.order_size[order_serial_num] = update_amount;
                log_data.add_log_data(i, dt, "update order amount", holding_data, order_data, performance_data);
            }
            else
            {
                Console.WriteLine("invalid update amount or order_serial_num in update_order_amount !");
            }

        }

        private void del_order(int order_serial_num, int i)
        {
            if (order_data.order_serial_list.Contains(order_serial_num))
            {
                order_data.order_serial_list.Remove(order_serial_num);
                order_data.order_side.Remove(order_serial_num);
                order_data.order_type.Remove(order_serial_num);
                order_data.order_size.Remove(order_serial_num);
                order_data.order_price.Remove(order_serial_num);
                order_data.order_i.Remove(order_serial_num);
                order_data.order_dt.Remove(order_serial_num);
                order_data.order_cancel.Remove(order_serial_num);
                order_data.order_message.Remove(order_serial_num);
            }
        }

        public void cancel_order(int order_serial_num, int i, string dt)
        {
            if (order_data.order_serial_list.Contains(order_serial_num))
            {
                if (order_data.order_cancel[order_serial_num] != true)
                {
                    order_data.order_cancel[order_serial_num] = true;
                    //order_data.order_i[order_serial_num] = i;
                }
                else
                {
                    Console.WriteLine("Cancel Failed!");
                }
            }
        }

        public void cancel_all_order(int i, string dt)
        {
            for (int s = 0; s < order_data.order_serial_list.Count; s++) { cancel_order(order_data.order_serial_list[s], i, dt); }
        }


        public void exit_all(int i, string dt)
        {
            if (holding_data.holding_size > 0)
                entry_order("market", holding_data.holding_side == "sell" ? "buy" : "sell", holding_data.holding_size, 0, i, dt, "exit all");
        }

        private void calc_fee(double size, double price, string maker_taker)
        {
            if (maker_taker == "maker")
            {
                performance_data.total_fee += price * size * maker_fee;
            }
            else if (maker_taker == "taker")
            {
                performance_data.total_fee += price * size * taker_fee;
            }
            else
            {
                Console.WriteLine("Invalid maker_taker type in calc_fee ! " + maker_taker);
            }
        }

        private void check_cancel(int i, string dt)
        {
            var serial_list = order_data.order_serial_list.ToArray();
            foreach (int s in serial_list)
            {
                //if (order_data.order_cancel[s] == true && order_data.order_i[s] < i)
                if (order_data.order_cancel[s] == true)
                {
                    del_order(s, i);
                    log_data.add_log_data(i, dt, "cancelled", holding_data, order_data, performance_data);
                }
            }
        }

        //order priceよりも低い・高い価格をつけたら約定と判定する。
        //buy: order price=10000のときにlowが9999.5以下になったら約定。
        //sell: order price=10000のときにhighが10000.5以上になったら約定。
        //market orderの約定値はbid/askにすべき。
        private void check_execution(int i, string dt, double open, double high, double low)
        {
            var serial_list = order_data.order_serial_list.ToArray();
            foreach (int s in serial_list)
            {
                if (order_data.order_type[s] == "market") //market orderのときはi関係なくclose valで約定
                {
                    //Console.WriteLine("executed market order"+ order_data.order_side[s] +  " @" + open.ToString() + ", i=" + i.ToString());
                    performance_data.num_maker_order++;
                    process_execution(open, s, i, dt);
                    del_order(s, i);
                }
                else if (order_data.order_i[s] < i)
                {
                    if (order_data.order_type[s] == "limit" && ((order_data.order_side[s] == "buy" && order_data.order_price[s] >= low + 0.5) || (order_data.order_side[s] == "sell" && order_data.order_price[s] <= high - 0.5)))
                    {
                        process_execution(order_data.order_price[s], s, i, dt);
                        del_order(s, i);
                    }
                }
            }
        }

        private void process_execution(double exec_price, int order_serial_num, int i, string dt)
        {
            if (order_data.order_side[order_serial_num] == "buy")
                log_data.buy_points[i] = exec_price;
            else
                log_data.sell_points[i] = exec_price;

            calc_fee(order_data.order_size[order_serial_num], exec_price, order_data.order_type[order_serial_num] == "limit" ? "maker" : "taker");
            if (holding_data.holding_side == "")
            {
                if (order_data.order_side[order_serial_num] == "buy")
                    performance_data.num_buy++;
                else
                    performance_data.num_sell++;
                holding_data.update_holding(order_data.order_side[order_serial_num], exec_price, order_data.order_size[order_serial_num], i);
                log_data.add_log_data(i, dt, "New Entry:" + order_data.order_type[order_serial_num], holding_data, order_data, performance_data);
            }
            else if (holding_data.holding_side == order_data.order_side[order_serial_num])
            {
                var ave_price = Math.Round(((holding_data.holding_price * holding_data.holding_size) + (exec_price * order_data.order_size[order_serial_num])) / (order_data.order_size[order_serial_num] + holding_data.holding_size), 1);
                holding_data.update_holding(holding_data.holding_side, ave_price, order_data.order_size[order_serial_num] + holding_data.holding_size, i);
                log_data.add_log_data(i, dt, "Additional Entry.", holding_data, order_data, performance_data);
            }
            else if (holding_data.holding_size > order_data.order_size[order_serial_num])
            {
                calc_executed_pl(exec_price, order_data.order_size[order_serial_num], i, false);
                holding_data.update_holding(holding_data.holding_side, holding_data.holding_price, holding_data.holding_size - order_data.order_size[order_serial_num], i);
                log_data.add_log_data(i, dt, "Exit Order (h>o)", holding_data, order_data, performance_data);
            }
            else if (holding_data.holding_size == order_data.order_size[order_serial_num])
            {
                calc_executed_pl(exec_price, order_data.order_size[order_serial_num], i, true);
                holding_data.holding_period_list.Add(holding_data.holding_period);
                holding_data.initialize_holding();
                log_data.add_log_data(i, dt, "Exit Order (h=o)", holding_data, order_data, performance_data);
            }
            else if (holding_data.holding_size < order_data.order_size[order_serial_num])
            {
                if (order_data.order_side[order_serial_num] == "buy")
                    performance_data.num_buy++;
                else
                    performance_data.num_sell++;
                calc_executed_pl(exec_price, holding_data.holding_size, i, true);
                
                holding_data.update_holding(order_data.order_side[order_serial_num], exec_price, order_data.order_size[order_serial_num] - holding_data.holding_size, i);
                log_data.add_log_data(i, dt, "'Exit & Entry Order (h<o)", holding_data, order_data, performance_data);
            }
            else
            {
                Console.WriteLine("Unknown situation in process execution !");
            }
        }

        private void calc_executed_pl(double exec_price, double size, int i, bool count_num_trade)
        {
            //var pl = holding_data.holding_side == "buy" ? ((exec_price - holding_data.holding_price) / holding_data.holding_price) * size : ((holding_data.holding_price - exec_price) / holding_data.holding_price) * size;
            var pl = holding_data.holding_side == "buy" ? (exec_price - holding_data.holding_price) * size : (holding_data.holding_price - exec_price) * size;
            //Console.WriteLine("pl="+pl.ToString() + ", i="+i.ToString());
            performance_data.realized_pl += Math.Round(pl, 6);
            performance_data.realized_pl_list.Add(Math.Round(pl, 6));

            if (count_num_trade)
            {
                performance_data.num_trade++;
                if (pl > 0) { performance_data.num_win++; }
            }
            if (holding_data.holding_side == "buy")
            {
                performance_data.buy_pl_list.Add(Math.Round(pl, 6));
                performance_data.buy_pl_ratio_list.Add(Math.Round(pl, 6) / MarketData.Close[i]);
            }
            else
            {
                performance_data.sell_pl_list.Add(Math.Round(pl, 6));
                performance_data.sell_pl_ratio_list.Add(Math.Round(pl, 6) / MarketData.Close[i]);
            }
        }



    }
}