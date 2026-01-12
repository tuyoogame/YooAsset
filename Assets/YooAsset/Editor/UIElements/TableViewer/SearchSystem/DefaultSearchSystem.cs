using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace YooAsset.Editor
{
    /// <summary>
    /// 默认的表格搜索系统，负责解析搜索表达式并执行数据过滤
    /// </summary>
    public static class DefaultSearchSystem
    {
        /// <summary>
        /// 解析搜索表达式为命令列表
        /// </summary>
        /// <param name="commandContent">搜索表达式字符串，支持关键字、比较运算符等语法</param>
        /// <returns>解析后的搜索命令集合</returns>
        public static IList<ISearchCommand> ParseCommand(string commandContent)
        {
            if (string.IsNullOrEmpty(commandContent))
                return new List<ISearchCommand>();

            List<ISearchCommand> results = new List<ISearchCommand>(10);
            string[] commands = Regex.Split(commandContent, @"\s+");
            foreach (var command in commands)
            {
                if (string.IsNullOrEmpty(command))
                    continue;

                if (command.Contains("!="))
                {
                    var splits = command.Split(new string[] { "!=" }, StringSplitOptions.None);
                    if (CheckSplitsValid(command, splits) == false)
                        continue;

                    var cmd = new SearchCompare();
                    cmd.HeaderTitle = splits[0];
                    cmd.CompareValue = splits[1];
                    cmd.CompareOperator = "!=";
                    results.Add(cmd);
                }
                else if (command.Contains(">="))
                {
                    var splits = command.Split(new string[] { ">=" }, StringSplitOptions.None);
                    if (CheckSplitsValid(command, splits) == false)
                        continue;

                    var cmd = new SearchCompare();
                    cmd.HeaderTitle = splits[0];
                    cmd.CompareValue = splits[1];
                    cmd.CompareOperator = ">=";
                    results.Add(cmd);
                }
                else if (command.Contains("<="))
                {
                    var splits = command.Split(new string[] { "<=" }, StringSplitOptions.None);
                    if (CheckSplitsValid(command, splits) == false)
                        continue;

                    var cmd = new SearchCompare();
                    cmd.HeaderTitle = splits[0];
                    cmd.CompareValue = splits[1];
                    cmd.CompareOperator = "<=";
                    results.Add(cmd);
                }
                else if (command.Contains(">"))
                {
                    var splits = command.Split('>');
                    if (CheckSplitsValid(command, splits) == false)
                        continue;

                    var cmd = new SearchCompare();
                    cmd.HeaderTitle = splits[0];
                    cmd.CompareValue = splits[1];
                    cmd.CompareOperator = ">";
                    results.Add(cmd);
                }
                else if (command.Contains("<"))
                {
                    var splits = command.Split('<');
                    if (CheckSplitsValid(command, splits) == false)
                        continue;

                    var cmd = new SearchCompare();
                    cmd.HeaderTitle = splits[0];
                    cmd.CompareValue = splits[1];
                    cmd.CompareOperator = "<";
                    results.Add(cmd);
                }
                else if (command.Contains("="))
                {
                    var splits = command.Split('=');
                    if (CheckSplitsValid(command, splits) == false)
                        continue;

                    var cmd = new SearchCompare();
                    cmd.HeaderTitle = splits[0];
                    cmd.CompareValue = splits[1];
                    cmd.CompareOperator = "=";
                    results.Add(cmd);
                }
                else if (command.Contains(":"))
                {
                    var splits = command.Split(':');
                    if (CheckSplitsValid(command, splits) == false)
                        continue;

                    var cmd = new SearchKeyword();
                    cmd.SearchTag = splits[0];
                    cmd.Keyword = splits[1];
                    results.Add(cmd);
                }
                else
                {
                    var cmd = new SearchKeyword();
                    cmd.SearchTag = string.Empty;
                    cmd.Keyword = command;
                    results.Add(cmd);
                }
            }
            return results;
        }
        private static bool CheckSplitsValid(string command, string[] splits)
        {
            if (splits.Length != 2)
            {
                Debug.LogWarning($"Invalid search command: '{command}'.");
                return false;
            }

            if (string.IsNullOrEmpty(splits[0]))
                return false;
            if (string.IsNullOrEmpty(splits[1]))
                return false;

            return true;
        }

        /// <summary>
        /// 根据搜索表达式过滤数据源的可见性
        /// </summary>
        /// <remarks>
        /// 搜索流程分为两个阶段：先按关键字过滤，再对通过的数据执行数值比较过滤。
        /// </remarks>
        /// <param name="sourceDatas">待过滤的表格数据集合</param>
        /// <param name="command">搜索表达式字符串</param>
        /// <param name="logic">同类搜索命令的组内组合逻辑</param>
        public static void Search(IList<ITableData> sourceDatas, string command, ESearchLogic logic)
        {
            var searchCmds = ParseCommand(command);
            var searchKeywordCmds = searchCmds.Where(cmd => cmd is SearchKeyword).ToList();
            var searchCompareCmds = searchCmds.Where(cmd => cmd is SearchCompare).ToList();

            foreach (var tableData in sourceDatas)
            {
                if (searchCmds.Count == 0)
                {
                    tableData.Visible = true;
                    continue;
                }

                // 优先匹配关键字
                if (SearchKeyword(tableData, searchKeywordCmds, logic) == false)
                {
                    tableData.Visible = false;
                    continue;
                }

                // 其次匹配数值
                if (SearchCompare(tableData, searchCompareCmds, logic) == false)
                {
                    tableData.Visible = false;
                    continue;
                }

                tableData.Visible = true;
            }
        }

        private static bool SearchKeyword(ITableData tableData, IList<ISearchCommand> commands, ESearchLogic logic)
        {
            if (commands.Count == 0)
                return true;

            if (logic == ESearchLogic.AND)
                return SearchKeywordAnd(tableData, commands);
            else if (logic == ESearchLogic.OR)
                return SearchKeywordOr(tableData, commands);
            else
                throw new System.NotImplementedException(logic.ToString());
        }
        private static bool SearchKeywordAnd(ITableData tableData, IList<ISearchCommand> commands)
        {
            foreach (var cmd in commands)
            {
                var searchKeywordCmd = cmd as SearchKeyword;
                bool matched = false;
                foreach (var tableCell in tableData.Cells)
                {
                    if (tableCell is StringValueCell stringValueCell)
                    {
                        if (string.IsNullOrEmpty(searchKeywordCmd.SearchTag) == false)
                        {
                            if (searchKeywordCmd.SearchTag == stringValueCell.SearchTag
                                && searchKeywordCmd.CompareTo(stringValueCell.StringValue))
                            {
                                matched = true;
                                break;
                            }
                        }
                        else
                        {
                            if (searchKeywordCmd.CompareTo(stringValueCell.StringValue))
                            {
                                matched = true;
                                break;
                            }
                        }
                    }
                }
                if (matched == false)
                    return false;
            }

            // 匹配成功
            return true;
        }
        private static bool SearchKeywordOr(ITableData tableData, IList<ISearchCommand> commands)
        {
            foreach (var tableCell in tableData.Cells)
            {
                foreach (var cmd in commands)
                {
                    var searchKeywordCmd = cmd as SearchKeyword;
                    if (tableCell is StringValueCell stringValueCell)
                    {
                        if (string.IsNullOrEmpty(searchKeywordCmd.SearchTag) == false)
                        {
                            if (searchKeywordCmd.SearchTag == stringValueCell.SearchTag
                                && searchKeywordCmd.CompareTo(stringValueCell.StringValue))
                                return true;
                        }
                        else
                        {
                            if (searchKeywordCmd.CompareTo(stringValueCell.StringValue))
                                return true;
                        }
                    }
                }
            }

            // 匹配失败
            return false;
        }

        private static bool SearchCompare(ITableData tableData, IList<ISearchCommand> commands, ESearchLogic logic)
        {
            if (commands.Count == 0)
                return true;

            if (logic == ESearchLogic.AND)
                return SearchCompareAnd(tableData, commands);
            else if (logic == ESearchLogic.OR)
                return SearchCompareOr(tableData, commands);
            else
                throw new System.NotImplementedException(logic.ToString());
        }
        private static bool SearchCompareAnd(ITableData tableData, IList<ISearchCommand> commands)
        {
            foreach (var cmd in commands)
            {
                var searchCompareCmd = cmd as SearchCompare;
                bool matched = false;
                foreach (var tableCell in tableData.Cells)
                {
                    if (tableCell is IntegerValueCell integerValueCell)
                    {
                        if (searchCompareCmd.HeaderTitle == integerValueCell.SearchTag
                            && searchCompareCmd.CompareTo(integerValueCell.IntegerValue))
                        {
                            matched = true;
                            break;
                        }
                    }
                    else if (tableCell is SingleValueCell singleValueCell)
                    {
                        if (searchCompareCmd.HeaderTitle == singleValueCell.SearchTag
                            && searchCompareCmd.CompareTo(singleValueCell.SingleValue))
                        {
                            matched = true;
                            break;
                        }
                    }
                }
                if (matched == false)
                    return false;
            }

            // 匹配成功
            return true;
        }
        private static bool SearchCompareOr(ITableData tableData, IList<ISearchCommand> commands)
        {
            foreach (var tableCell in tableData.Cells)
            {
                foreach (var cmd in commands)
                {
                    var searchCompareCmd = cmd as SearchCompare;
                    if (tableCell is IntegerValueCell integerValueCell)
                    {
                        if (searchCompareCmd.HeaderTitle == integerValueCell.SearchTag
                            && searchCompareCmd.CompareTo(integerValueCell.IntegerValue))
                            return true;
                    }
                    else if (tableCell is SingleValueCell singleValueCell)
                    {
                        if (searchCompareCmd.HeaderTitle == singleValueCell.SearchTag
                            && searchCompareCmd.CompareTo(singleValueCell.SingleValue))
                            return true;
                    }
                }
            }

            // 匹配失败
            return false;
        }
    }
}