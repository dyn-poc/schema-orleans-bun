namespace schema.Grains;

using Json.Schema;
using Json.Schema.Generation;

public class SchemaConvert
{
    public static JsonSchemaBuilder Convert(Dictionary<string, Field> fields, string root) => new Schema(fields, root).ToTree().Apply(new JsonSchemaBuilder().Id(root));
}

public record Schema(Dictionary<string, Field> Fields,
    string Root="root")
{
    [Required] public Dictionary<string, Field> Fields { get; init; } = Fields;
    public FieldNode ToTree()
    {
        var fields = from field in this.Fields
            let chain = field.Key.Split('.')
            let nodes = getTreeNodes(chain, (this.Root, this.Root))
            select nodes;

        var allFields = fields.SelectMany(f => f).Distinct().ToArray();

        return new FieldNode(GetChildren(this.Root), this.Root, this.Root);


        IEnumerable<KeyValuePair<string, FieldNode>> GetChildren(string id)
        {
            foreach (var (child_id, name,_) in allFields.Where(f => f.parent.id == id))
            {
                yield return new KeyValuePair<string, FieldNode>(name,
                    new FieldNode(GetChildren(child_id), name, child_id) { Field = FieldDef(child_id), });
            }
        }

        Field? FieldDef(string id) => this.Fields.TryGetValue(id, out var def) ? def : null;

        static IEnumerable<(string id, string name, (string id, string name) parent )> getTreeNodes(string[] nodes, (string id, string name) root)
        {
            var ids = GetIds(nodes).Zip(nodes).Select((node, index) => (id: node.First, name: node.Second)).ToArray();
            var tree = from index in Enumerable.Range(1, ids.Length)
                let parents = ids[..index]
                let children = ids[index..]
                let parent = parents.Length > 1 ? parents[^2] : root
                let current = parents[^1]
                select (current.id, current.name, parent, parents: parents.SkipLast(1).ToArray(), children);

            foreach (var (id, name, parent, _, _) in tree)
            {
                yield return (id, name, parent);
            }
        }

        static IEnumerable<string> GetIds(string[] nodes) =>
            from index in Enumerable.Range(1, nodes.Length)
            select string.Join('.', nodes[..index]);
    }

 }
