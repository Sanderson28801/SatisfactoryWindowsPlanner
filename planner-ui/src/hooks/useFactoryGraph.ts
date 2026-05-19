import { useState } from "react";
import { useNodesState, useEdgesState, type Edge, type Node } from "reactflow";
import { type GraphDto, type NodeDto } from "../types";
import { getLayoutedElements } from "../utils/graphLayout";

const API_BASE = "https://localhost:7299/api/factory";

export type CalculatePayload = {
  targetItemId: string;
  targetAmountPerMinute: number;
  unlockedAlternates: string[];
};

export function useFactoryGraph() {
  const [nodes, setNodes, onNodesChange] = useNodesState([]);
  const [edges, setEdges, onEdgesChange] = useEdgesState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const calculateGraph = async (payload: CalculatePayload) => {
    if (!payload.targetItemId) {
      setError("Please select an item first!");
      return;
    }

    setLoading(true);
    setError(null);

    const apiPayload = {
      ...payload,
      byproductPenaltyWeight: 80,
      scavengeRewardWeight: 20,
    };

    try {
      const response = await fetch(`${API_BASE}/calculate`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(apiPayload),
      });

      if (!response.ok) {
        const errData = await response.json();
        throw new Error(errData.error || "Calculation failed");
      }

      const data: GraphDto = await response.json();
      const nodeMap = new Map<string, NodeDto>(
        data.nodes.map((n) => [n.id, n])
      );

      // --- GRAPH COMPRESSION & BRIDGING ---
      const bypassedNodes = new Set<string>();
      const finalNodesList: NodeDto[] = [];

      data.nodes.forEach((n) => {
        if (n.type === "Item") {
          const hasIncoming = data.edges.some((e) => e.target === n.id);
          const hasOutgoing = data.edges.some((e) => e.source === n.id);
          if (hasIncoming && hasOutgoing) bypassedNodes.add(n.id);
          else finalNodesList.push(n);
        } else finalNodesList.push(n);
      });

      const finalEdgesList: any[] = [];
      data.edges.forEach((edge) => {
        if (
          !bypassedNodes.has(edge.source) &&
          !bypassedNodes.has(edge.target)
        ) {
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

      // --- MAP TO VISUAL NODES ---
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

      const { nodes: layoutedNodes, edges: layoutedEdges } =
        getLayoutedElements(initialNodes, initialEdges, "LR");
      setNodes(layoutedNodes);
      setEdges(layoutedEdges);
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return {
    nodes,
    edges,
    onNodesChange,
    onEdgesChange,
    loading,
    error,
    calculateGraph,
  };
}
