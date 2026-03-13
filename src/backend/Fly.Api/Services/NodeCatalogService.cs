using Fly.Api.Models;

namespace Fly.Api.Services;

public static class NodeCatalogService
{
    public static IReadOnlyList<NodeDefinition> GetAll() => Catalog;

    private static readonly List<NodeDefinition> Catalog =
    [
        new NodeDefinition
        {
            Type        = "start",
            Category    = "控制",
            Label       = "开始",
            Description = "流程的起始节点",
            Icon        = "Play",
            Color       = "#67C23A",
            Parameters  = []
        },
        new NodeDefinition
        {
            Type        = "end",
            Category    = "控制",
            Label       = "结束",
            Description = "流程的终止节点",
            Icon        = "CircleCheck",
            Color       = "#909399",
            Parameters  = []
        },
        new NodeDefinition
        {
            Type        = "decision",
            Category    = "控制",
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
            Type        = "wait",
            Category    = "控制",
            Label       = "等待",
            Description = "暂停流程一段时间",
            Icon        = "Timer",
            Color       = "#909399",
            Parameters  =
            [
                new NodeParameterDef { Key = "duration_sec", Label = "等待时长", Type = "number", DefaultValue = 30, Required = true, Unit = "秒" }
            ]
        },
        new NodeDefinition
        {
            Type        = "pipetting",
            Category    = "液体处理",
            Label       = "移液",
            Description = "使用液体处理工作站进行移液操作",
            Icon        = "Aim",
            Color       = "#409EFF",
            Parameters  =
            [
                new NodeParameterDef { Key = "volume_ul",   Label = "移液量",   Type = "number",  DefaultValue = 100,      Required = true,  Unit = "µL" },
                new NodeParameterDef { Key = "source",      Label = "来源位置", Type = "string",  DefaultValue = "源板",   Required = true  },
                new NodeParameterDef { Key = "target",      Label = "目标位置", Type = "string",  DefaultValue = "目标板", Required = true  },
                new NodeParameterDef { Key = "tip_type",    Label = "吸头类型", Type = "select",  DefaultValue = "standard",
                    Options = ["standard", "filtered", "wide-bore"] },
                new NodeParameterDef { Key = "mix_cycles",  Label = "混匀次数", Type = "number",  DefaultValue = 0 }
            ]
        },
        new NodeDefinition
        {
            Type        = "incubation",
            Category    = "样品处理",
            Label       = "孵育",
            Description = "在指定温度下孵育一段时间",
            Icon        = "Sunny",
            Color       = "#F56C6C",
            Parameters  =
            [
                new NodeParameterDef { Key = "temperature_c", Label = "温度",     Type = "number", DefaultValue = 37,  Required = true, Unit = "°C" },
                new NodeParameterDef { Key = "duration_min",  Label = "孵育时长", Type = "number", DefaultValue = 60,  Required = true, Unit = "分钟" },
                new NodeParameterDef { Key = "humidity",      Label = "湿度",     Type = "number", DefaultValue = 95,  Unit = "%" },
                new NodeParameterDef { Key = "co2",           Label = "CO₂浓度",  Type = "number", DefaultValue = 5,   Unit = "%" }
            ]
        },
        new NodeDefinition
        {
            Type        = "centrifugation",
            Category    = "样品处理",
            Label       = "离心",
            Description = "离心样品",
            Icon        = "Refresh",
            Color       = "#9B59B6",
            Parameters  =
            [
                new NodeParameterDef { Key = "speed_rpm",    Label = "转速",     Type = "number", DefaultValue = 3000, Required = true, Unit = "rpm" },
                new NodeParameterDef { Key = "duration_min", Label = "离心时长", Type = "number", DefaultValue = 5,    Required = true, Unit = "分钟" },
                new NodeParameterDef { Key = "temperature_c",Label = "温度",     Type = "number", DefaultValue = 4,    Unit = "°C" },
                new NodeParameterDef { Key = "acceleration",  Label = "加速度",   Type = "number", DefaultValue = 9,
                    Description = "1-9 级别" }
            ]
        },
        new NodeDefinition
        {
            Type        = "mixing",
            Category    = "样品处理",
            Label       = "混合",
            Description = "混合或振荡样品",
            Icon        = "Operation",
            Color       = "#1ABC9C",
            Parameters  =
            [
                new NodeParameterDef { Key = "mode",         Label = "混合方式", Type = "select", DefaultValue = "vortex",
                    Options = ["vortex", "orbital-shake", "rocker", "roller"], Required = true },
                new NodeParameterDef { Key = "speed_rpm",    Label = "速度",     Type = "number", DefaultValue = 600,  Unit = "rpm" },
                new NodeParameterDef { Key = "duration_sec", Label = "时长",     Type = "number", DefaultValue = 30,   Required = true, Unit = "秒" }
            ]
        },
        new NodeDefinition
        {
            Type        = "washing",
            Category    = "样品处理",
            Label       = "洗涤",
            Description = "洗板/洗涤样品",
            Icon        = "WaterDrop",
            Color       = "#3498DB",
            Parameters  =
            [
                new NodeParameterDef { Key = "cycles",       Label = "洗涤次数",         Type = "number", DefaultValue = 3,   Required = true },
                new NodeParameterDef { Key = "volume_ul",    Label = "每次洗涤量",       Type = "number", DefaultValue = 300, Required = true, Unit = "µL" },
                new NodeParameterDef { Key = "soak_sec",     Label = "浸泡时间",         Type = "number", DefaultValue = 0,   Unit = "秒" },
                new NodeParameterDef { Key = "wash_buffer",  Label = "洗涤液",           Type = "string", DefaultValue = "PBS-T" }
            ]
        },
        new NodeDefinition
        {
            Type        = "detection",
            Category    = "检测",
            Label       = "检测",
            Description = "使用读板仪或其他检测设备进行测量",
            Icon        = "DataAnalysis",
            Color       = "#E67E22",
            Parameters  =
            [
                new NodeParameterDef { Key = "mode",           Label = "检测模式", Type = "select", DefaultValue = "absorbance",
                    Options = ["absorbance", "fluorescence", "luminescence", "time-resolved-fluorescence"], Required = true },
                new NodeParameterDef { Key = "wavelength_nm",  Label = "波长",     Type = "number", DefaultValue = 450, Unit = "nm" },
                new NodeParameterDef { Key = "reference_nm",   Label = "参考波长", Type = "number", Unit = "nm", Description = "可选参考波长" },
                new NodeParameterDef { Key = "output_file",    Label = "结果文件", Type = "string", DefaultValue = "result.csv" }
            ]
        }
    ];
}
