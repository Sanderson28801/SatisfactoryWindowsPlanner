import { type Edge, type Node } from "reactflow";
import { type GraphDto, type NodeDto } from "../types";

export function parseFactoryGraph(data: GraphDto): {
  initialNodes: Node[];
  initialEdges: Edge[];
} {
  const nodeMap = new Map<string, NodeDto>(data.nodes.map((n) => [n.id, n]));
  const bypassedNodes = new Set<string>();
  const finalNodesList: NodeDto[] = [];
  const finalEdgesList: any[] = [];

  // 1. COMPRESSION
  data.nodes.forEach((n) => {
    if (n.type === "Item") {
      const hasIncoming = data.edges.some((e) => e.target === n.id);
      const hasOutgoing = data.edges.some((e) => e.source === n.id);
      if (hasIncoming && hasOutgoing) bypassedNodes.add(n.id);
      else finalNodesList.push(n);
    } else finalNodesList.push(n);
  });

  // 2. BRIDGING
  data.edges.forEach((edge) => {
    if (!bypassedNodes.has(edge.source) && !bypassedNodes.has(edge.target)) {
      finalEdgesList.push({ ...edge, bridgedItemId: null });
      return;
    }
    if (bypassedNodes.has(edge.source)) {
      const hiddenItemId = edge.source;
      const consumerId = edge.target;
      data.edges
        .filter((e) => e.target === hiddenItemId)
        .forEach((p) => {
          finalEdgesList.push({
            source: p.source,
            target: consumerId,
            amount: edge.amount,
            bridgedItemId: hiddenItemId,
          });
        });
    }
  });

  // 3. MAPPING
  const initialNodes: Node[] = finalNodesList.map((n) => {
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
    const outgoingEdge = data.edges.find((e) => e.source === n.id);
    const iconItemId =
      n.type === "Recipe" && outgoingEdge ? outgoingEdge.target : n.id;

    return {
      id: n.id,
      type: n.type === "Recipe" ? "recipeNode" : "default",
      position: { x: 0, y: 0 },
      data: {
        label: n.label,
        itemId: iconItemId.slice(5),
        machines: n.machines,
        power: n.power,
        inputs,
        outputs,
      },
      style:
        n.type === "Item"
          ? {
              backgroundColor: "#1e3a8a",
              color: "#fff",
              border: "1px solid #3b82f6",
              fontWeight: "bold",
              borderRadius: "8px",
              padding: "10px",
            }
          : undefined,
    };
  });

  const initialEdges: Edge[] = finalEdgesList.map((e) => {
    const name = e.bridgedItemId
      ? nodeMap.get(e.bridgedItemId)?.label || "Item"
      : (nodeMap.get(e.source)?.type === "Item"
          ? nodeMap.get(e.source)?.label
          : nodeMap.get(e.target)?.label) || "Item";
    return {
      id: `${e.source}-${e.target}-${e.bridgedItemId || "direct"}`,
      source: e.source,
      target: e.target,
      label: `${name}: ${e.amount.toFixed(1)}/m`,
      animated: true,
      labelStyle: { fill: "#fff", fontWeight: 600, fontSize: "10px" },
      labelBgStyle: { fill: "#1f2937", fillOpacity: 0.9 },
      labelBgPadding: [6, 4],
      labelBgBorderRadius: 4,
      style: { stroke: "#4b5563", strokeWidth: 2 },
    };
  });

  return { initialNodes, initialEdges };
}
