import { useEffect, useState } from "react";
import ReactFlow, {
  Background,
  Controls,
  type Edge,
  type Node,
} from "reactflow";
import dagre from "dagre";
import "reactflow/dist/style.css"; // Don't forget the CSS!

// 1. MATCH YOUR C# DTOs EXACTLY
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

// 2. THE DAGRE AUTO-LAYOUT ENGINE
const getLayoutedElements = (
  nodes: Node[],
  edges: Edge[],
  direction = "LR"
) => {
  const dagreGraph = new dagre.graphlib.Graph();
  dagreGraph.setDefaultEdgeLabel(() => ({}));
  dagreGraph.setGraph({ rankdir: direction });

  // Add nodes to dagre
  nodes.forEach((node) => {
    dagreGraph.setNode(node.id, { width: 250, height: 100 }); // Approximate width/height of a card
  });

  // Add edges to dagre
  edges.forEach((edge) => {
    dagreGraph.setEdge(edge.source, edge.target);
  });

  // Run the math
  dagre.layout(dagreGraph);

  // Apply the calculated X/Y coordinates back to React Flow nodes
  nodes.forEach((node) => {
    const nodeWithPosition = dagreGraph.node(node.id);
    node.position = {
      x: nodeWithPosition.x - 250 / 2,
      y: nodeWithPosition.y - 100 / 2,
    };
  });

  return { nodes, edges };
};

export default function App() {
  const [nodes, setNodes] = useState<Node[]>([]);
  const [edges, setEdges] = useState<Edge[]>([]);
  const [loading, setLoading] = useState(true);

  // 3. FETCH AND BUILD GRAPH ON LOAD
  useEffect(() => {
    // Note: Using the secure port 7261 from your console logs!
    const url =
      "https://localhost:7261/api/factory/calculate?item=Desc_Battery_C&amount=50";

    fetch(url)
      .then((res) => res.json())
      .then((data: GraphDto) => {
        // Convert C# NodeDto into React Flow Node
        const initialNodes: Node[] = data.nodes.map((n) => ({
          id: n.id,
          position: { x: 0, y: 0 }, // Dagre will fix this in a second
          data: {
            // We use string interpolation to show the amount/machines on the label!
            label: `${n.label}\n${
              n.type === "Item"
                ? n.amount + " / min"
                : n.machines + "x Machines"
            }`,
          },
          // Color code Recipes vs Items using Tailwind hex codes
          style: {
            backgroundColor: n.type === "Item" ? "#e0f2fe" : "#fef08a",
            fontWeight: "bold",
          },
        }));

        // Convert C# EdgeDto into React Flow Edge
        const initialEdges: Edge[] = data.edges.map((e) => ({
          id: `${e.source}-${e.target}`,
          source: e.source,
          target: e.target,
          label: `${e.amount} / min`, // Draw the transfer rate on the wire!
          animated: true, // Makes the wires flow visually
        }));

        // Run the auto-layout
        const { nodes: layoutedNodes, edges: layoutedEdges } =
          getLayoutedElements(
            initialNodes,
            initialEdges,
            "LR" // Left to Right layout
          );

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
      <div className="flex h-screen items-center justify-center font-bold text-2xl">
        Loading Factory...
      </div>
    );

  // 4. THE UI RENDER
  return (
    // React Flow requires a parent container with an explicit width and height!
    <div className="w-screen h-screen bg-neutral-900">
      <ReactFlow
        nodes={nodes}
        edges={edges}
        fitView // Automatically zooms camera to fit the whole graph
      >
        <Background color="#555" gap={16} />
        <Controls />
      </ReactFlow>
    </div>
  );
}
