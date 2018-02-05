using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NRules.RuleModel;
using NRules.RuleModel.Builders;
using System.Dynamic;
using NRule.Domain;

namespace NRule.Rule
{
    public class CustomRuleRepository : IRuleRepository
    {
        private readonly IRuleSet _ruleSet = new RuleSet("DefaultRuleSet");
        Dictionary<string, List<AlarmRules>> ruleItems = null;

        public CustomRuleRepository(Dictionary<string, List<AlarmRules>> ruleItems)
        {
            this.ruleItems = ruleItems;
        }
        private static List<string> getMsgLsit(Dictionary<string, List<AlarmRules>> ruleItems)
        {
            List<string> msgIdList = new List<string>();
            foreach (KeyValuePair<string, List<AlarmRules>> kv in ruleItems)
            {
                msgIdList.Add(kv.Key);
            }
            return msgIdList;
        }

        public IEnumerable<IRuleSet> GetRuleSets()
        {
            return new[] { _ruleSet };
        }

        public void LoadRules()
        {
            List<IRuleDefinition> ruleDefinitionList = new List<IRuleDefinition>();

            foreach (KeyValuePair<string, List<AlarmRules>> ruleItem in ruleItems)
            {
                List<string> msgIdList = getMsgLsit(ruleItems);
                if (ruleItem.Value != null)
                {
                    foreach (AlarmRules ar in ruleItem.Value)
                    {
                        ruleDefinitionList.Add(BuildRule(ruleItem.Key, ar, msgIdList));
                    }
                }
            }
            _ruleSet.Add(ruleDefinitionList);
        }

        private IRuleDefinition BuildRule(string msgID, AlarmRules ruleItem, List<string> msgIdList)
        {
            var builder = new NRules.RuleModel.Builders.RuleBuilder();
            builder.Name("Load MsgID " + msgID + " AlarmID " + ruleItem.AlarmRuleId);
            PatternBuilder expandoPattern = builder.LeftHandSide().Pattern(typeof(ExpandoObject), "expando");

            Expression<Func<ExpandoObject, bool>> expandoCondition = expando => msgIdMatch(expando, msgIdList);
            expandoPattern.Condition(expandoCondition);

            Expression<Func<ExpandoObject, bool>> expandoCondition1 = expando => runRule(expando, ruleItem, msgID);
            expandoPattern.Condition(expandoCondition1);

            Expression<Action<IContext, ExpandoObject>> action =
             (ctx, expando) => Console.WriteLine("Triggered--- MsgID:" + msgID + " AlarmID " + ruleItem.AlarmRuleId + outPut(expando));

            builder.RightHandSide().Action(action);
            return builder.Build();
        }

        private bool msgIdMatch(ExpandoObject o, List<string> msgIdList)
        {
            string msgstr = "";
            var expandoDict = o as IDictionary<string, object>;
            if (expandoDict.ContainsKey("MessageId"))
            {
                msgstr = expandoDict["MessageId"].ToString();

            }
            return msgIdList.Contains(msgstr);
        }
        private bool valueMatch(ExpandoObject o, RuleItem ri)
        {
            bool result = false;
            double value = 0;
            var expandoDict = o as IDictionary<string, object>;
            if (expandoDict.ContainsKey(ri.ElmentName))
            {
                if (double.TryParse(expandoDict[ri.ElmentName].ToString(), out value))
                {
                    switch (ri.Operation)
                    {
                        case "=":
                            result = (value == ri.RightNumbericValue);
                            break;
                        case ">":
                            result = (value > ri.RightNumbericValue);
                            break;
                        case "<":
                            result = (value < ri.RightNumbericValue);
                            break;
                        case ">=":
                            result = (value >= ri.RightNumbericValue);
                            break;
                        case "<=":
                            result = (value <= ri.RightNumbericValue);
                            break;
                        case "!=":
                            result = (value != ri.RightNumbericValue);
                            break;
                        default:
                            throw new ArgumentNullException();
                    }
                }
                else
                {
                    throw new TypeAccessException();
                }
            }
            else
            {
                throw new ArgumentNullException();
            }
            return result;
        }
        private bool stringMatch(ExpandoObject o, RuleItem ri)
        {
            bool result = false;
            string str = "";
            var expandoDict = o as IDictionary<string, object>;
            if (expandoDict.ContainsKey(ri.ElmentName))
            {
                str = expandoDict[ri.ElmentName].ToString();
            }
            switch (ri.Operation)
            {
                case "=":
                    result = str.Equals(ri.RightStringValue.ToString());
                    break;
                case "!=":
                    result = str != ri.RightStringValue.ToString();
                    break;
                default:
                    throw new ArgumentNullException();
            }
            return result;

        }
        private bool boolMatch(ExpandoObject o, RuleItem ri)
        {
            bool result = false;
            bool trueOrfalse = false;
            var expandoDict = o as IDictionary<string, object>;
            if (expandoDict.ContainsKey(ri.ElmentName))
            {
                if (bool.TryParse(expandoDict[ri.ElmentName].ToString(), out trueOrfalse))
                {

                    switch (ri.Operation)
                    {
                        case "=":
                            result = trueOrfalse.Equals((ri.RightboolValue));
                            break;
                        case "!=":
                            result = trueOrfalse.Equals(!(ri.RightboolValue));
                            break;
                        default:
                            throw new ArgumentNullException();
                    }
                }
                else
                {
                    throw new TypeAccessException();
                }
            }
            else
            {
                throw new ArgumentNullException();
            }
            return result;
        }
        private bool runRule(ExpandoObject o, AlarmRules ruleItem, string msgID)
        {
            string telemetryMsgId = "";
            bool result = false;
            var expandoDict = o as IDictionary<string, object>;
            if (expandoDict.ContainsKey("MessageId"))
            {
                telemetryMsgId = expandoDict["MessageId"].ToString();
            }
            if (telemetryMsgId == msgID)
            {
                result = rule(o, ruleItem);
            }
            return result;
        }
        private bool rule(ExpandoObject o, AlarmRules ar)
        {
            bool result = false;
            //getresult 
            foreach (RuleItem ri in ar.RuleItemlist)
            {
                switch (ri.DataType)
                {
                    case 0:
                        ri.result = boolMatch(o, ri);
                        break;
                    case 1:
                        ri.result = valueMatch(o, ri);
                        break;
                    case 2:
                        ri.result = stringMatch(o, ri);
                        break;
                    default:
                        throw new ArgumentNullException();
                }
            }
            result = compileBitWiseRules(ar.RuleItemlist.Count - 1, ar.RuleItemlist);
            return result;
        }
        private string outPut(ExpandoObject o)
        {
            string name = "";
            var expandoDict = o as IDictionary<string, object>;
            if (expandoDict.ContainsKey("MessageId"))
            {

                name = "  " + expandoDict["MessageId"].ToString();
                name += " " + expandoDict["MessageName"].ToString() + " CompanyID: " + expandoDict["companyId"].ToString() + " " + expandoDict["msgTimestamp"].ToString();

            }
            return name;
        }
        private bool compileBitWiseRules(int offset, List<RuleItem> ruleItems)
        {
            RuleItem rei = ruleItems[offset];
            if (offset == 0)
            {
                return rei.result;
            }
            else
            {
                offset--;
                RuleItem previousRei = ruleItems[offset];
                return ComplieBoolRule(rei.result, previousRei.Logic, compileBitWiseRules(offset, ruleItems));
            }
        }
        public static bool ComplieBoolRule(bool left, string op, bool right)
        {
            switch (op)
            {
                case "AND":
                    return left & right;
                case "OR":
                    return left | right;
                case "XOR":
                    return left ^ right;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}