import dagre from "dagre";
import { type Edge, type Node } from "reactflow";

export const getLayoutedElements = (
  nodes: Node[],
  edges: Edge[],
  direction = "LR"
) => {
  const dagreGraph = new dagre.graphlib.Graph();
  dagreGraph.setDefaultEdgeLabel(() => ({}));
  dagreGraph.setGraph({ rankdir: direction });

  nodes.forEach((node) =>
    dagreGraph.setNode(node.id, { width: 260, height: 110 })
  );
  edges.forEach((edge) => dagreGraph.setEdge(edge.source, edge.target));
  dagre.layout(dagreGraph);

  nodes.forEach((node) => {
    const nodeWithPosition = dagreGraph.node(node.id);
    node.position = {
      x: nodeWithPosition.x - 260 / 2,
      y: nodeWithPosition.y - 110 / 2,
    };
  });
  return { nodes, edges };
};
