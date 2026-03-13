using Fly.Api.Models;

namespace Fly.Api.Services;

public static class NodeCatalogService
{
    public static IReadOnlyList<NodeDefinition> GetAll() => Catalog;

    // Only built-in flow control steps
    private static readonly List<NodeDefinition> Catalog =
    [
        new NodeDefinition
        {
            Type        = "start",
            Category    = "流程控制",
            Label       = "开始",
            Description = "流程的起始节点",
            Icon        = "Play",
            Color       = "#67C23A",
            Parameters  = []
        },
        new NodeDefinition
        {
            Type        = "end",
            Category    = "流程控制",
            Label       = "结束",
            Description = "流程的终止节点",
            Icon        = "CircleCheck",
            Color       = "#909399",
            Parameters  = []
        },
        new NodeDefinition
        {
            Type        = "decision",
            Category    = "流程控制",
            Label       = "条件判断",
            Description = "根据条件选择后续分支",
            Icon        = "Share",
            Color       = "#E6A23C",
            Parameters  =
            [
                new NodeParameterDef { Key = "condition", Label = "判断条件", Type = "string", Required = true, Description = "例如：result > 0.5" }
            ]
        },
        new NodeDefinition
        {
            Type        = "loop",
            Category    = "流程控制",
            Label       = "循环",
            Description = "重复执行一组操作",
            Icon        = "RefreshRight",
            Color       = "#409EFF",
            Parameters  =
            [
                new NodeParameterDef { Key = "iterations", Label = "循环次数", Type = "number", DefaultValue = 1, Required = true, Description = "执行循环的次数" },
                new NodeParameterDef { Key = "condition", Label = "循环条件", Type = "string", Description = "可选的循环条件表达式" }
            ]
        },
        new NodeDefinition
        {
            Type        = "break",
            Category    = "流程控制",
            Label       = "跳出循环",
            Description = "终止当前循环",
            Icon        = "Close",
            Color       = "#F56C6C",
            Parameters  = []
        },
        new NodeDefinition
        {
            Type        = "continue",
            Category    = "流程控制",
            Label       = "继续循环",
            Description = "跳过本次循环，继续下一次",
            Icon        = "Right",
            Color       = "#409EFF",
            Parameters  = []
        },
        new NodeDefinition
        {
            Type        = "parallel",
            Category    = "流程控制",
            Label       = "并行",
            Description = "同时执行多个操作",
            Icon        = "Grid",
            Color       = "#1ABC9C",
            Parameters  =
            [
                new NodeParameterDef { Key = "max_concurrent", Label = "最大并发数", Type = "number", DefaultValue = 2, Description = "同时执行的最大任务数" }
            ]
        },
        new NodeDefinition
        {
            Type        = "async",
            Category    = "流程控制",
            Label       = "异步",
            Description = "启动异步任务",
            Icon        = "Notification",
            Color       = "#9B59B6",
            Parameters  =
            [
                new NodeParameterDef { Key = "task_id", Label = "任务ID", Type = "string", Required = true, Description = "异步任务的唯一标识" }
            ]
        },
        new NodeDefinition
        {
            Type        = "wait_async",
            Category    = "流程控制",
            Label       = "等待异步完成",
            Description = "等待异步任务完成",
            Icon        = "Clock",
            Color       = "#E67E22",
            Parameters  =
            [
                new NodeParameterDef { Key = "task_id", Label = "任务ID", Type = "string", Required = true, Description = "要等待的异步任务ID" },
                new NodeParameterDef { Key = "timeout_sec", Label = "超时时间", Type = "number", DefaultValue = 300, Unit = "秒", Description = "等待超时时间" }
            ]
        },
        new NodeDefinition
        {
            Type        = "wait",
            Category    = "流程控制",
            Label       = "等待",
            Description = "暂停流程一段时间",
            Icon        = "Timer",
            Color       = "#909399",
            Parameters  =
            [
                new NodeParameterDef { Key = "duration_sec", Label = "等待时长", Type = "number", DefaultValue = 30, Required = true, Unit = "秒" }
            ]
        }
    ];
}
