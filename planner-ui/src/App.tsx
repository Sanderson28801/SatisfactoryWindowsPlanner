import { useEffect, useState } from "react";
import ReactFlow, {
  Background,
  Controls,
  Handle,
  Position,
  type Edge,
  type Node,
} from "reactflow";
import dagre from "dagre";
import "reactflow/dist/style.css";

type NodeDto = {
  id: string;
  label: string;
  type: string;
  amount: number;
  machines: number;
  power: number;
};
type EdgeDto = { source: string; target: string; amount: number };
type GraphDto = { nodes: NodeDto[]; edges: EdgeDto[] };

const getLayoutedElements = (
  nodes: Node[],
  edges: Edge[],
  direction = "LR"
) => {
  const dagreGraph = new dagre.graphlib.Graph();
  dagreGraph.setDefaultEdgeLabel(() => ({}));
  dagreGraph.setGraph({ rankdir: direction });

  nodes.forEach((node) => {
    dagreGraph.setNode(node.id, { width: 250, height: 100 });
  });

  edges.forEach((edge) => {
    dagreGraph.setEdge(edge.source, edge.target);
  });

  dagre.layout(dagreGraph);

  nodes.forEach((node) => {
    const nodeWithPosition = dagreGraph.node(node.id);
    node.position = {
      x: nodeWithPosition.x - 250 / 2,
      y: nodeWithPosition.y - 100 / 2,
    };
  });

  return { nodes, edges };
};

const RecipeNode = ({ data }: { data: any }) => {
  return (
    <div className="group relative px-4 py-2 shadow-md rounded-md bg-yellow-200 border-2 border-yellow-400 font-bold text-center text-black min-w-[150px]">
      <Handle type="target" position={Position.Left} className="w-2 h-2" />

      <div>{data.label}</div>
      <div className="text-xs font-normal">{data.machines}x Machines</div>
      <div className="text-xs font-normal text-yellow-800">{data.power} MW</div>

      <div className="absolute top-full left-1/2 -translate-x-1/2 mt-2 w-48 bg-neutral-800 text-white text-xs rounded-md shadow-xl p-3 opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none z-50">
        {data.inputs.length > 0 && (
          <div className="mb-2">
            <div className="font-bold border-b border-neutral-600 pb-1 mb-1 text-red-400">
              Inputs
            </div>
            {data.inputs.map((i: any, idx: number) => (
              <div key={idx} className="flex justify-between">
                <span>{i.name}</span>
                <span>{i.amount}/m</span>
              </div>
            ))}
          </div>
        )}
        {data.outputs.length > 0 && (
          <div>
            <div className="font-bold border-b border-neutral-600 pb-1 mb-1 text-green-400 mt-2">
              Outputs
            </div>
            {data.outputs.map((o: any, idx: number) => (
              <div key={idx} className="flex justify-between">
                <span>{o.name}</span>
                <span>{o.amount}/m</span>
              </div>
            ))}
          </div>
        )}
      </div>
      <Handle type="source" position={Position.Right} className="w-2 h-2" />
    </div>
  );
};

const nodeTypes = { recipeNode: RecipeNode };

export default function App() {
  const [nodes, setNodes] = useState<Node[]>([]);
  const [edges, setEdges] = useState<Edge[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const url =
      "https://localhost:7261/api/factory/calculate?item=Desc_Battery_C&amount=50";

    fetch(url)
      .then((res) => res.json())
      .then((data: GraphDto) => {
        const nodeMap = new Map<string, NodeDto>(
          data.nodes.map((n) => [n.id, n])
        );

        // --- 1. GRAPH COMPRESSION: Find nodes to hide ---
        const bypassedNodes = new Set<string>();
        const finalNodesList: NodeDto[] = [];

        data.nodes.forEach((n) => {
          if (n.type === "Item") {
            const hasIncoming = data.edges.some((e) => e.target === n.id);
            const hasOutgoing = data.edges.some((e) => e.source === n.id);

            // If it is produced AND consumed, it's intermediate. Hide it!
            if (hasIncoming && hasOutgoing) {
              bypassedNodes.add(n.id);
            } else {
              finalNodesList.push(n); // Keep Root items and Raw Materials
            }
          } else {
            finalNodesList.push(n); // Keep all Recipes
          }
        });

        // --- 2. BRIDGE THE EDGES ---
        const finalEdgesList: any[] = [];
        data.edges.forEach((edge) => {
          // If this is a normal edge (neither end is hidden), keep it
          if (
            !bypassedNodes.has(edge.source) &&
            !bypassedNodes.has(edge.target)
          ) {
            finalEdgesList.push({ ...edge, bridgedItemId: null });
            return;
          }

          // If this edge goes FROM a hidden item TO a consumer...
          if (bypassedNodes.has(edge.source)) {
            const itemId = edge.source;
            const consumerId = edge.target;
            const amountNeeded = edge.amount;

            // Find the recipe that made this hidden item, and draw a direct line!
            const producers = data.edges.filter((e) => e.target === itemId);
            producers.forEach((p) => {
              finalEdgesList.push({
                source: p.source,
                target: consumerId,
                amount: amountNeeded,
                bridgedItemId: itemId, // Save the item ID so we know what to label the wire
              });
            });
          }
        });

        // --- 3. MAP TO REACT FLOW ---
        const initialNodes: Node[] = finalNodesList.map((n) => {
          // TOOLTIP MAGIC: We still calculate inputs/outputs using the RAW uncompressed data!
          const inputs = data.edges
            .filter((e) => e.target === n.id)
            .map((e) => ({
              name: nodeMap.get(e.source)?.label || "Unknown",
              amount: e.amount,
            }));
          const outputs = data.edges
            .filter((e) => e.source === n.id)
            .map((e) => ({
              name: nodeMap.get(e.target)?.label || "Unknown",
              amount: e.amount,
            }));

          return {
            id: n.id,
            type: n.type === "Recipe" ? "recipeNode" : "default",
            position: { x: 0, y: 0 },
            data: {
              label: `${n.label}\n${
                n.type === "Item" ? n.amount + " / min" : ""
              }`,
              machines: n.machines,
              power: n.power,
              inputs,
              outputs,
            },
            style:
              n.type === "Item"
                ? { backgroundColor: "#e0f2fe", fontWeight: "bold" }
                : undefined,
          };
        });

        const initialEdges: Edge[] = finalEdgesList.map((e) => {
          // Figure out the name for the label on the wire
          let productName = "Item";
          if (e.bridgedItemId) {
            productName = nodeMap.get(e.bridgedItemId)?.label || "Item";
          } else {
            const sourceNode = nodeMap.get(e.source);
            const targetNode = nodeMap.get(e.target);
            const itemNode =
              sourceNode?.type === "Item" ? sourceNode : targetNode;
            productName = itemNode ? itemNode.label : "Item";
          }

          return {
            // Create a guaranteed unique ID so React Flow doesn't complain about duplicates
            id: `${e.source}-${e.target}-${e.bridgedItemId || "direct"}`,
            source: e.source,
            target: e.target,
            label: `${productName}: ${e.amount} / min`,
            animated: true,
            labelStyle: { fill: "#111", fontWeight: 700 },
            labelBgStyle: { fill: "#f8fafc", fillOpacity: 0.8 },
            labelBgPadding: [8, 4],
            labelBgBorderRadius: 4,
          };
        });

        const { nodes: layoutedNodes, edges: layoutedEdges } =
          getLayoutedElements(initialNodes, initialEdges, "LR");

        setNodes(layoutedNodes);
        setEdges(layoutedEdges);
        setLoading(false);
      })
      .catch((err) => {
        console.error("Failed to fetch graph:", err);
        setLoading(false);
      });
  }, []);

  if (loading)
    return (
      <div className="flex h-screen items-center justify-center font-bold text-2xl text-white bg-neutral-900">
        Loading Factory...
      </div>
    );

  return (
    <div className="w-screen h-screen bg-neutral-900">
      <ReactFlow nodes={nodes} edges={edges} nodeTypes={nodeTypes} fitView>
        <Background color="#555" gap={16} />
        <Controls />
      </ReactFlow>
    </div>
  );
}
