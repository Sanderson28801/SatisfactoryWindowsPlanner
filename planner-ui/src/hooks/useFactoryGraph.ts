import { useState } from "react";
import { useNodesState, useEdgesState } from "reactflow";
import { type GraphDto } from "../types";
import { parseFactoryGraph } from "../utils/graphParser";
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
    if (!payload.targetItemId) return setError("Please select an item first!");
    setLoading(true);
    setError(null);

    try {
      const response = await fetch(`${API_BASE}/calculate`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          ...payload,
          byproductPenaltyWeight: 80,
          scavengeRewardWeight: 20,
        }),
      });

      if (!response.ok)
        throw new Error((await response.json()).error || "Calculation failed");
      const data: GraphDto = await response.json();

      // Look how clean this is now!
      const { initialNodes, initialEdges } = parseFactoryGraph(data);
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
