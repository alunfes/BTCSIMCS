using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BTCSIM
{
    public class Sim
    {
        public Sim()
        {
        }

        public SimAccount sim_ga(int from, int to, Gene2 chromo, SimAccount ac)
        {
            var nn = new NN();
            var strategy = new Strategy();
            int amount = 1;
            var nn_input_data_generator = new NNInputDataGenerator();

            for (int i = from; i < to - 1; i++)
            {
                var nn_inputs = nn_input_data_generator.generateNNInputData(ac, i);
                var nn_outputs = nn.calcNN(nn_inputs, chromo, 0);
                var pred = nn.getActivatedUnit(nn_outputs);
                var actions = strategy.GAStrategy(pred, amount, ac);
                for (int j = 0; j < actions.action.Count; j++)
                {
                    if (actions.action[j] == "entry")
                        ac.entry_order(actions.order_type[j], actions.order_side[j], actions.order_size[j], actions.order_price[j], i, MarketData.Dt[i].ToString(), actions.order_message[j]);
                }
                ac.move_to_next(i + 1, MarketData.Dt[i + 1].ToString(), MarketData.Open[i + 1], MarketData.High[i + 1], MarketData.Low[i + 1], MarketData.Close[i + 1]);
            }
            ac.last_day(to, MarketData.Close[to]);
            ac.calc_sharp_ratio();
            return ac;
        }

        public SimAccount sim_ga_limit(int from, int to, int max_amount, Gene2 chromo, SimAccount ac)
        {
            var nn = new NN();
            var strategy = new Strategy();
            int amount = 1;
            var nn_input_data_generator = new NNInputDataGenerator();

            for (int i = from; i < to - 1; i++)
            {
                var nn_inputs = nn_input_data_generator.generateNNInputDataLimit(ac, i, chromo.num_index);
                var nn_outputs = nn.calcNN(nn_inputs, chromo, 0);
                var pred = nn.getActivatedUnit(nn_outputs);
                var actions = strategy.GALimitStrategy2(i, pred, amount, max_amount, ac);

                //check invalid ac situation
                if (ac.order_data.order_side.Count > 1)
                    Console.WriteLine("Sim: # of order is more than 1 !");

                for (int j = 0; j < actions.action.Count; j++)
                {
                    if (actions.action[j] == "entry")
                        ac.entry_order(actions.order_type[j], actions.order_side[j], actions.order_size[j], actions.order_price[j], i, MarketData.Dt[i].ToString(), actions.order_message[j]);
                    else if (actions.action[j] == "cancel")
                        ac.cancel_all_order(i, MarketData.Dt[i].ToString());
                    else if (actions.action[j] == "update amount")
                        ac.update_order_amount(actions.order_size[j], actions.order_serial_num[j], i, MarketData.Dt[i].ToString());
                    else if (actions.action[j] == "update price")
                        ac.update_order_price(actions.order_price[j], actions.order_serial_num[j], i, MarketData.Dt[i].ToString());
                    else
                        Console.WriteLine("Sim: Unknown strategy action !");
                }
                ac.move_to_next(i + 1, MarketData.Dt[i + 1].ToString(), MarketData.Open[i + 1], MarketData.High[i + 1], MarketData.Low[i + 1], MarketData.Close[i + 1]);
            }
            ac.last_day(to, MarketData.Close[to]);
            ac.calc_sharp_ratio();
            return ac;
        }


        public SimAccount sim_ga_market_limit(int from, int to, int max_amount, Gene2 chromo, SimAccount ac, double nn_threshold, bool stop_no_trade)
        {
            var nn = new NN();
            var strategy = new Strategy();
            int amount = 1;
            var nn_input_data_generator = new NNInputDataGenerator();
            var zero_trade_check_point = 0.1; //to - fromの間のx%進捗した時にnum trade=0だったらsimを打ち切る。

            for (int i = from; i < to - 1; i++)
            {
                //check num trade=0 discontinue sim
                if (stop_no_trade && i - from > ((to - from) * zero_trade_check_point) && ac.performance_data.num_trade == 0)
                {
                    //Console.WriteLine("Stopped sim due to zero trade.");
                    break;
                }

                var nn_inputs = nn_input_data_generator.generateNNInputDataLimit(ac, i, chromo.num_index);
                var nn_outputs = nn.calcNN(nn_inputs, chromo, 0);
                //var pred = nn.getActivatedUnitLimitMarket(nn_outputs, nn_threshold);
                var pred = nn.getActivatedUnitLimitMarket2(nn_outputs, nn_threshold);
                var actions = strategy.GALimitMarketStrategy(i, pred, amount, max_amount, ac);

                //check invalid ac situation
                if (ac.order_data.order_side.Count > 1)
                    Console.WriteLine("Sim: # of order is more than 1 !");

                for (int j = 0; j < actions.action.Count; j++)
                {
                    if (actions.action[j] == "entry")
                        ac.entry_order(actions.order_type[j], actions.order_side[j], actions.order_size[j], actions.order_price[j], i, MarketData.Dt[i].ToString(), actions.order_message[j]);
                    else if (actions.action[j] == "cancel")
                        ac.cancel_all_order(i, MarketData.Dt[i].ToString());
                    else if (actions.action[j] == "update amount")
                        ac.update_order_amount(actions.order_size[j], actions.order_serial_num[j], i, MarketData.Dt[i].ToString());
                    else if (actions.action[j] == "update price")
                        ac.update_order_price(actions.order_price[j], actions.order_serial_num[j], i, MarketData.Dt[i].ToString());
                    else
                        Console.WriteLine("Sim: Unknown strategy action !");
                }
                ac.move_to_next(i + 1, MarketData.Dt[i + 1].ToString(), MarketData.Open[i + 1], MarketData.High[i + 1], MarketData.Low[i + 1], MarketData.Close[i + 1]);
            }
            ac.last_day(to, MarketData.Close[to]);
            ac.calc_sharp_ratio();
            return ac;
        }


        public SimAccount sim_win_ga_market(int from, int to, Gene2 chromo, SimAccount ac, double nn_threshold)
        {
            var nn = new NN();
            var strategy = new Strategy();
            var nn_input_data_generator = new NNInputDataGenerator();

            for (int i = from; i < to - 1; i++)
            {
                var nn_inputs = nn_input_data_generator.generateNNWinGA(i, chromo.num_index);
                var nn_outputs = nn.calcNN(nn_inputs, chromo, 0);
                var pred = nn.getActivatedUnitOnlyBuySell(nn_outputs, nn_threshold);
                var actions = strategy.GAWinMarketStrategy(i, pred, ac);
                for (int j = 0; j < actions.action.Count; j++)
                {
                    if (actions.action[j] == "entry")
                        ac.entry_order(actions.order_type[j], actions.order_side[j], actions.order_size[j], actions.order_price[j], i, MarketData.Dt[i].ToString(), actions.order_message[j]);
                }
                ac.move_to_next(i + 1, MarketData.Dt[i + 1].ToString(), MarketData.Open[i + 1], MarketData.High[i + 1], MarketData.Low[i + 1], MarketData.Close[i + 1]);
            }
            ac.last_day(to, MarketData.Close[to]);
            ac.calc_sharp_ratio();
            return ac;
        }



        /*
         *
         */
        public SimAccount sim_entry_timing_ptlc(int from, int to, SimAccount ac, string side, double max_leverage, int entry_interval_miniute, int entry_num,  double pt_ratio, double lc_ratio)
        {
            if (side == "buy" || side == "sell")
            {
                var strategy = new Strategy();
                for (int i = from; i < to - 1; i++)
                {
                    var max_size = max_leverage * ac.performance_data.total_capital / MarketData.Close[i]; //常にレバ２倍になるようにサイズを決める
                    var actions = strategy.entryTimingPTLCStrategy(i, max_size, ac, side, entry_num, entry_interval_miniute, pt_ratio, lc_ratio);
                    for (int j = 0; j < actions.action.Count; j++)
                    {
                        if (actions.action[j] == "entry")
                            ac.entry_order(actions.order_type[j], actions.order_side[j], actions.order_size[j], actions.order_price[j], i, MarketData.Dt[i].ToString(), actions.order_message[j]);
                    }
                    ac.move_to_next(i + 1, MarketData.Dt[i + 1].ToString(), MarketData.Open[i + 1], MarketData.High[i + 1], MarketData.Low[i + 1], MarketData.Close[i + 1]);
                }
                ac.last_day(to, MarketData.Close[to]);
                ac.calc_sharp_ratio();
            }
            else
                Console.WriteLine("Invalid side! " + side);
            return ac;
        }


        //エントリータイミングをx分にy%以上変動した時などに予め限定してエントリーする戦略
        public SimAccount sim_entry_timing_ptlc_price_change(int from, int to, SimAccount ac, double max_leverage, int entry_interval_miniute, int entry_num, double pt_ratio, double lc_ratio, int buy_price_change_minutes, double buy_price_change_ratio, int sell_price_change_minutes, double sell_price_change_ratio)
        {
            var strategy = new Strategy();
            for (int i = from; i < to - 1; i++)
            {
                var max_size = max_leverage * ac.performance_data.total_capital / MarketData.Close[i]; //常にレバ２倍になるようにサイズを決める
                var actions = strategy.entryTimingPTLCPriceChangeStrategy(i, max_size, ac, entry_num, entry_interval_miniute, pt_ratio, lc_ratio, buy_price_change_minutes, buy_price_change_ratio, sell_price_change_minutes, sell_price_change_ratio);
                for (int j = 0; j < actions.action.Count; j++)
                {
                    if (actions.action[j] == "entry")
                        ac.entry_order(actions.order_type[j], actions.order_side[j], actions.order_size[j], actions.order_price[j], i, MarketData.Dt[i].ToString(), actions.order_message[j]);
                }
                ac.move_to_next(i + 1, MarketData.Dt[i + 1].ToString(), MarketData.Open[i + 1], MarketData.High[i + 1], MarketData.Low[i + 1], MarketData.Close[i + 1]);
            }
            ac.last_day(to, MarketData.Close[to]);
            ac.calc_sharp_ratio();
            return ac;
        }
    }
}