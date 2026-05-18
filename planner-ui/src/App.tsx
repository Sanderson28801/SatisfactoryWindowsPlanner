import { useState, useEffect } from "react";
import ReactFlow, {
  Background,
  Controls,
  Handle,
  Position,
  useNodesState,
  useEdgesState,
  type Edge,
  type Node,
} from "reactflow";
import dagre from "dagre";
import "reactflow/dist/style.css";

// DTO and Data Types
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
type MetaItem = { id: string; name: string };

const getLayoutedElements = (
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
// Safe helper to convert "Desc_Cement_C" to "/icons/desc-cement-c_64.png"
const getIconPath = (itemId: string | undefined): string => {
  // if (!itemId) return "/icons/placeholder.png";

  const formattedId = itemId.toLowerCase().replaceAll("_", "-");
  return `/icons/${formattedId}_64.png`;
};

// --- CUSTOM NODE WITH IMAGES ---
const RecipeNode = ({ data }: { data: any }) => {
  return (
    <div className="group relative px-4 py-3 shadow-2xl rounded-xl bg-neutral-800 border-2 border-orange-500/50 text-white min-w-[220px] transition-all hover:border-orange-500">
      <Handle
        type="target"
        position={Position.Left}
        className="w-3 h-3 bg-orange-500"
      />

      <div className="flex items-center gap-3">
        {/* Dynamic Image from public/icons/className.png */}
        <img
          src={getIconPath(data.itemId)}
          onError={(e) => {
            (e.target as HTMLImageElement).src = "/icons/placeholder.png";
          }}
          className="w-10 h-10 object-contain bg-neutral-900 rounded-lg p-1 border border-neutral-700"
          alt=""
        />
        <div className="text-left">
          <div className="font-bold text-sm text-neutral-100 truncate max-w-[140px]">
            {data.label}
          </div>
          <div className="text-xs text-neutral-400">
            {data.machines.toFixed(2)}x Machines
          </div>
          <div className="text-xs font-semibold text-orange-400">
            {data.power.toFixed(1)} MW
          </div>
        </div>
      </div>

      {/* Tooltip on Hover */}
      <div className="absolute top-full left-1/2 -translate-x-1/2 mt-2 w-56 bg-neutral-950 text-white text-xs rounded-lg shadow-2xl p-3 opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none z-50 border border-neutral-800">
        {data.inputs.length > 0 && (
          <div className="mb-2">
            <div className="font-bold border-b border-neutral-800 pb-1 mb-1 text-red-400">
              Inputs
            </div>
            {data.inputs.map((i: any, idx: number) => (
              <div key={idx} className="flex justify-between py-0.5">
                <span>{i.name}</span>
                <span className="text-neutral-400">
                  {i.amount.toFixed(1)}/m
                </span>
              </div>
            ))}
          </div>
        )}
        {data.outputs.length > 0 && (
          <div>
            <div className="font-bold border-b border-neutral-800 pb-1 mb-1 text-green-400">
              Outputs
            </div>
            {data.outputs.map((o: any, idx: number) => (
              <div key={idx} className="flex justify-between py-0.5">
                <span>{o.name}</span>
                <span className="text-neutral-400">
                  {o.amount.toFixed(1)}/m
                </span>
              </div>
            ))}
          </div>
        )}
      </div>
      <Handle
        type="source"
        position={Position.Right}
        className="w-3 h-3 bg-orange-500"
      />
    </div>
  );
};

const nodeTypes = { recipeNode: RecipeNode };
const API_BASE = "https://localhost:7299/api/factory";

export default function App() {
  // React Flow hooks handle dragging state automatically!
  const [nodes, setNodes, onNodesChange] = useNodesState([]);
  const [edges, setEdges, onEdgesChange] = useEdgesState([]);

  // Metadata Indices
  const [availableItems, setAvailableItems] = useState<MetaItem[]>([]);
  const [alternateRecipes, setAlternateRecipes] = useState<MetaItem[]>([]);

  // App UI States
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // User Selection Choices
  const [searchQuery, setSearchQuery] = useState("");
  const [selectedItemId, setSelectedItemId] = useState("");
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);
  const [amountInput, setAmountInput] = useState<string>("10"); // Keeps string to prevent 0-prefix bugs
  const [unlockedAlternates, setUnlockedAlternates] = useState<string[]>([]);

  // Load Metadata indices on Startup
  useEffect(() => {
    fetch(`${API_BASE}/items`)
      .then((res) => res.json())
      .then(setAvailableItems)
      .catch(console.error);
    fetch(`${API_BASE}/alternates`)
      .then((res) => res.json())
      .then(setAlternateRecipes)
      .catch(console.error);
  }, []);

  const filteredItems = availableItems.filter((item) =>
    item.name.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const handleToggleAlternate = (recipeId: string) => {
    setUnlockedAlternates((prev) =>
      prev.includes(recipeId)
        ? prev.filter((id) => id !== recipeId)
        : [...prev, recipeId]
    );
  };

  const handleCalculate = async () => {
    if (!selectedItemId) {
      setError("Please select an item first!");
      return;
    }
    setLoading(true);
    setError(null);

    const payload = {
      targetItemId: selectedItemId,
      targetAmountPerMinute: parseFloat(amountInput) || 0,
      unlockedAlternates: unlockedAlternates,
      byproductPenaltyWeight: 80,
      scavengeRewardWeight: 20,
    };

    try {
      const response = await fetch(`${API_BASE}/calculate`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(payload),
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

        // FIX: If this is a Recipe node, find the first item it outputs to use as the icon image!
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

  return (
    <div className="flex h-screen w-screen bg-neutral-950 text-neutral-200 overflow-hidden font-sans">
      {/* SIDEBAR */}
      <div className="w-85 bg-neutral-900 border-r border-neutral-800 p-6 flex flex-col justify-between shadow-2xl z-20">
        <div className="flex flex-col gap-5 overflow-y-auto pr-1">
          <h1 className="text-xl font-black tracking-wider text-orange-500 uppercase border-b border-neutral-800 pb-3">
            FICSIT Assembly Planner
          </h1>

          {/* SEARCHABLE DROP-DOWN SELECTOR */}
          <div className="relative">
            <label className="block text-xs font-bold uppercase tracking-wider text-neutral-400 mb-1.5">
              Target Output Product
            </label>
            <div
              onClick={() => setIsDropdownOpen(!isDropdownOpen)}
              className="w-full bg-neutral-950 border border-neutral-700 rounded-lg px-3 py-2.5 text-left flex items-center justify-between cursor-pointer hover:border-neutral-500 transition-colors"
            >
              <div className="flex items-center gap-2 truncate">
                {selectedItemId && (
                  <img
                    src={getIconPath(selectedItemId)}
                    onError={(e) => {
                      (e.target as any).src = "/icons/placeholder.png";
                    }}
                    className="w-5 h-5 object-contain"
                    alt=""
                  />
                )}
                <span>
                  {selectedItemId
                    ? availableItems.find((i) => i.id === selectedItemId)?.name
                    : "Choose an item..."}
                </span>
              </div>
              <span className="text-neutral-500 text-xs">▼</span>
            </div>

            {isDropdownOpen && (
              <div className="absolute top-full left-0 w-full mt-1 bg-neutral-950 border border-neutral-800 rounded-lg shadow-2xl z-50 max-h-60 overflow-y-auto p-2 flex flex-col gap-1">
                <input
                  type="text"
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  onClick={(e) => e.stopPropagation()} // Keeps dropdown open while typing
                  className="w-full bg-neutral-900 border border-neutral-700 rounded px-2.5 py-1.5 text-sm text-white mb-2 focus:outline-none focus:border-orange-500"
                  placeholder="Type to filter..."
                />
                {filteredItems.map((item) => (
                  <div
                    key={item.id}
                    onClick={() => {
                      setSelectedItemId(item.id);
                      setIsDropdownOpen(false);
                      setSearchQuery("");
                    }}
                    className="flex items-center gap-2 px-2 py-1.5 rounded hover:bg-orange-500 hover:text-white cursor-pointer transition-colors text-sm truncate"
                  >
                    <img
                      src={getIconPath(item.id)}
                      onError={(e) => {
                        (e.target as any).src = "/icons/placeholder.png";
                      }}
                      className="w-5 h-5 object-contain"
                      alt=""
                    />
                    <span>{item.name}</span>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* AMOUNT INPUT (FIXED PRE-FIX VALUE LOGIC) */}
          <div>
            <label className="block text-xs font-bold uppercase tracking-wider text-neutral-400 mb-1.5">
              Target Yield (Units / Min)
            </label>
            <input
              type="number"
              value={amountInput}
              onChange={(e) => setAmountInput(e.target.value)} // String tracking completely eliminates 0-prefixes
              className="w-full bg-neutral-950 border border-neutral-700 rounded-lg px-3 py-2 text-white focus:outline-none focus:border-orange-500 transition-colors font-mono"
              min="0.01"
            />
          </div>

          {/* DYNAMIC ALTERNATE RECIPES CHECKLIST PANEL */}
          <div className="flex flex-col flex-1">
            <label className="block text-xs font-bold uppercase tracking-wider text-neutral-400 mb-2">
              Unlocked Alternate Recipes
            </label>
            <div className="flex-1 min-h-[180px] bg-neutral-950 border border-neutral-800 rounded-lg p-3 overflow-y-auto flex flex-col gap-2">
              {alternateRecipes.length === 0 ? (
                <div className="text-xs text-neutral-600 italic p-2 text-center">
                  Loading alternate database...
                </div>
              ) : (
                alternateRecipes.map((recipe) => (
                  <label
                    key={recipe.id}
                    className={`flex items-center gap-3 p-2 rounded-md cursor-pointer border select-none transition-all text-xs ${
                      unlockedAlternates.includes(recipe.id)
                        ? "bg-orange-950/30 border-orange-500/40 text-orange-300"
                        : "bg-neutral-900/40 border-transparent hover:border-neutral-800 text-neutral-400"
                    }`}
                  >
                    <input
                      type="checkbox"
                      checked={unlockedAlternates.includes(recipe.id)}
                      onChange={() => handleToggleAlternate(recipe.id)}
                      className="accent-orange-500 w-3.5 h-3.5 rounded"
                    />
                    <div className="truncate pr-1">
                      <div className="font-semibold truncate">
                        {recipe.name}
                      </div>
                      <div className="text-[10px] text-neutral-500 font-mono truncate">
                        {recipe.id}
                      </div>
                    </div>
                  </label>
                ))
              )}
            </div>
          </div>
        </div>

        {/* CONTROLS AREA */}
        <div className="pt-4 border-t border-neutral-800 mt-4">
          <button
            onClick={handleCalculate}
            disabled={loading}
            className="w-full bg-orange-600 hover:bg-orange-500 disabled:bg-neutral-800 disabled:text-neutral-600 text-neutral-950 font-black tracking-wide uppercase py-3 px-4 rounded-lg shadow-xl transition-colors cursor-pointer"
          >
            {loading ? "Analyzing Schematic..." : "Initialize Factory"}
          </button>
          {error && (
            <div className="mt-3 p-3 bg-red-950/40 border border-red-900 rounded-lg text-red-300 text-xs font-medium">
              {error}
            </div>
          )}
        </div>
      </div>

      {/* REACT FLOW CANVAS */}
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
            onNodesChange={onNodesChange} // Hook mapping enables drag-and-drop mechanics!
            onEdgesChange={onEdgesChange}
            fitView
          >
            <Background color="#262626" gap={18} size={1} />
            <Controls className="bg-neutral-900 border border-neutral-800 text-white rounded-lg p-1 fill-white [&_button]:border-neutral-800" />
          </ReactFlow>
        )}
      </div>
    </div>
  );
}
