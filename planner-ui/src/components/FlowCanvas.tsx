import ReactFlow, {
  Background,
  Controls,
  type Node,
  type Edge,
  type OnNodesChange,
  type OnEdgesChange,
} from "reactflow";
import { RecipeNode } from "./nodes/RecipeNode"; // Adjust path to where you saved it
import "reactflow/dist/style.css";

const nodeTypes = { recipeNode: RecipeNode };

type FlowCanvasProps = {
  nodes: Node[];
  edges: Edge[];
  onNodesChange: OnNodesChange;
  onEdgesChange: OnEdgesChange;
  loading: boolean;
};

export function FlowCanvas({
  nodes,
  edges,
  onNodesChange,
  onEdgesChange,
  loading,
}: FlowCanvasProps) {
  return (
    <div className="flex-1 h-full relative">
      {nodes.length === 0 && !loading ? (
        <div className="absolute inset-0 flex flex-col items-center justify-center text-neutral-600 font-bold tracking-wide pointer-events-none gap-2">
          <span className="text-4xl">⚙</span>
          <span>Awaiting FICSIT Production Orders</span>
        </div>
      ) : (
        <ReactFlow
          nodes={nodes}
          edges={edges}
          nodeTypes={nodeTypes}
          onNodesChange={onNodesChange}
          onEdgesChange={onEdgesChange}
          fitView
        >
          <Background color="#262626" gap={18} size={1} />
          <Controls className="bg-neutral-900 border border-neutral-800 text-white rounded-lg p-1 fill-white [&_button]:border-neutral-800" />
        </ReactFlow>
      )}
    </div>
  );
}
