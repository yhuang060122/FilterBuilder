using System.Text.Json.Nodes;

public static class DevExtremeFilterParser
{
    public static FilterGroup Parse(JsonArray filter) => ParseGroup(filter);

    private static FilterGroup ParseGroup(JsonArray array)
    {
        if(IsNotGroup(array))
        {
            var inner = ParseGroup((JsonArray)array[1]!);
            inner.Not = true;
            return inner;
        }

        var andGroups = SplitByOr(array)
            .Select(ParseAndGroup)
            .ToList();
        
        return andGroups.Count == 1
        ? andGroups[0]
        : new FilterGroup
        {
            Operator = LogicalOperator.Or,
            Groups = andGroups
        };
    }

    private static FilterGroup ParseAndGroup(List<JsonNode?> nodes)
    {
        var group = new FilterGroup
        {
            Operator = LogicalOperator.And
        };

        foreach(var node in nodes)
        {
            var array = (JsonArray)node!;

            if(IsRule(array))
            {
                group.Rules.Add(ParseRule(array));
            }
            else
            {
                group.Groups.Add(ParseGroup(array));
            }
        }

        return group;
    }


    private static List<List<JsonNode?>> SplitByOr(JsonArray array)
    {
        var result = new List<List<JsonNode?>>();
        var current = new List<JsonNode?>();

        foreach(var node in array)
        {
            if(node is JsonValue value && value.TryGetValue<string>(out var token))
            {
                switch(token)
                {
                    case "or":
                        result.Add(current);
                        current = new();
                        break;
                    case "and":
                        break;
                    default:
                        throw new NotSupportedException(
                            $"Unexpected token: {token}");
                }
                continue;
            }

            current.Add(node);
        }

        result.Add(current);

        return result;
    }



    private static bool IsNotGroup(JsonArray array)
        => array.Count  == 2 &&
            array[0] is JsonValue value &&
            value.TryGetValue<string>(out var token) &&
            token == "!";

    private static bool IsRule(JsonArray array)
        => array.Count == 3 &&
            array[0] is JsonValue &&
            array[1] is JsonValue;

    private static FilterRule ParseRule(JsonArray array)
        => new FilterRule
        {
            Property = array[0]!.GetValue<string>(),
            Operator = MapOperator(array[1]!.GetValue<string>()),
            Value = GetValue(array[2])
        };

    private static object? GetValue(JsonNode? node)
    {
        if (node == null) return null;

        if (node is JsonValue value)
        {
            if (value.TryGetValue<string>(out var s)) return s;
            if (value.TryGetValue<int>(out var i)) return i;
            if (value.TryGetValue<long>(out var l)) return l;
            if (value.TryGetValue<decimal>(out var d)) return d;
            if (value.TryGetValue<double>(out var db)) return db;
            if (value.TryGetValue<bool>(out var b)) return b;
        }

        return node.ToJsonString();
    }

    private static ComparisonOperator MapOperator(string op)
    {
        return op switch
        {
            "=" => ComparisonOperator.Equal,
            "<>" => ComparisonOperator.NotEqual,
            ">" => ComparisonOperator.GreaterThan,
            ">=" => ComparisonOperator.GreaterThanOrEqual,
            "<" => ComparisonOperator.LessThan,
            "<=" => ComparisonOperator.LessThanOrEqual,
            "contains" => ComparisonOperator.Contains,
            "startswith" => ComparisonOperator.StartsWith,
            "endswith" => ComparisonOperator.EndsWith,
            _ => throw new NotSupportedException($"Operator {op}")
        };
    }
}