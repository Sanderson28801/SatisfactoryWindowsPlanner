import { useState } from "react";
import { Sidebar } from "./components/Sidebar";
import { FlowCanvas } from "./components/FlowCanvas";
import { useFactoryData } from "./hooks/useFactoryData";
import { useFactoryGraph } from "./hooks/useFactoryGraph";

export default function App() {
  // 1. Call the Data Hook
  // This automatically fetches your items and alternates on mount
  const { items, alternates } = useFactoryData();

  // 2. Call the Graph Hook
  // This gives you the React Flow state and the calculation trigger
  const {
    nodes,
    edges,
    onNodesChange,
    onEdgesChange,
    loading,
    error,
    calculateGraph,
  } = useFactoryGraph();

  // 3. Local UI State
  // We keep this in App.tsx so both the Sidebar (to display them)
  // and the Graph hook (to calculate with them) have access to these values.
  const [selectedItemId, setSelectedItemId] = useState("");
  const [amountInput, setAmountInput] = useState("10");
  const [unlockedAlternates, setUnlockedAlternates] = useState<string[]>([]);

  // 4. State Handlers
  const handleToggleAlternate = (recipeId: string) => {
    setUnlockedAlternates((prev) =>
      prev.includes(recipeId)
        ? prev.filter((id) => id !== recipeId)
        : [...prev, recipeId]
    );
  };

  const handleInitialize = () => {
    calculateGraph({
      targetItemId: selectedItemId,
      targetAmountPerMinute: parseFloat(amountInput) || 0,
      unlockedAlternates,
    });
  };

  // 5. The Render
  return (
    <div className="flex h-screen w-screen bg-neutral-950 text-neutral-200 overflow-hidden font-sans">
      {/* Feed the Sidebar its options, its current state, and its update functions */}
      <Sidebar
        items={items}
        alternates={alternates}
        selectedItemId={selectedItemId}
        onSelectItem={setSelectedItemId}
        amountInput={amountInput}
        onAmountChange={setAmountInput}
        unlockedAlternates={unlockedAlternates}
        onToggleAlternate={handleToggleAlternate}
        onCalculate={handleInitialize}
        loading={loading}
        error={error}
      />

      {/* Feed the Canvas the graph arrays and the drag-and-drop handlers */}
      <FlowCanvas
        nodes={nodes}
        edges={edges}
        onNodesChange={onNodesChange}
        onEdgesChange={onEdgesChange}
        loading={loading}
      />
    </div>
  );
}
