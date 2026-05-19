import { useState } from "react";
import { type MetaItem } from "../types";
import { getIconPath } from "../utils/formatter";

type SidebarProps = {
  items: MetaItem[];
  alternates: MetaItem[];
  selectedItemId: string;
  onSelectItem: (id: string) => void;
  amountInput: string;
  onAmountChange: (val: string) => void;
  unlockedAlternates: string[];
  onToggleAlternate: (id: string) => void;
  onCalculate: () => void;
  loading: boolean;
  error: string | null;
};

export function Sidebar(props: SidebarProps) {
  const [searchQuery, setSearchQuery] = useState("");
  const [isDropdownOpen, setIsDropdownOpen] = useState(false);

  const filteredItems = props.items.filter((item) =>
    item.name.toLowerCase().includes(searchQuery.toLowerCase())
  );

  return (
    <div className="w-85 bg-neutral-900 border-r border-neutral-800 p-6 flex flex-col justify-between shadow-2xl z-20">
      <div className="flex flex-col gap-5 overflow-y-auto pr-1">
        <h1 className="text-xl font-black tracking-wider text-orange-500 uppercase border-b border-neutral-800 pb-3">
          FICSIT Assembly Planner
        </h1>

        <div className="relative">
          <label className="block text-xs font-bold uppercase tracking-wider text-neutral-400 mb-1.5">
            Target Output Product
          </label>
          <div
            onClick={() => setIsDropdownOpen(!isDropdownOpen)}
            className="w-full bg-neutral-950 border border-neutral-700 rounded-lg px-3 py-2.5 text-left flex items-center justify-between cursor-pointer hover:border-neutral-500 transition-colors"
          >
            <div className="flex items-center gap-2 truncate">
              {props.selectedItemId && (
                <img
                  src={getIconPath(props.selectedItemId)}
                  onError={(e) =>
                    ((e.target as any).src = "/icons/placeholder.png")
                  }
                  className="w-5 h-5 object-contain"
                  alt=""
                />
              )}
              <span>
                {props.selectedItemId
                  ? props.items.find((i) => i.id === props.selectedItemId)?.name
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
                onClick={(e) => e.stopPropagation()}
                className="w-full bg-neutral-900 border border-neutral-700 rounded px-2.5 py-1.5 text-sm text-white mb-2 focus:outline-none focus:border-orange-500"
                placeholder="Type to filter..."
              />
              {filteredItems.map((item) => (
                <div
                  key={item.id}
                  onClick={() => {
                    props.onSelectItem(item.id);
                    setIsDropdownOpen(false);
                    setSearchQuery("");
                  }}
                  className="flex items-center gap-2 px-2 py-1.5 rounded hover:bg-orange-500 hover:text-white cursor-pointer transition-colors text-sm truncate"
                >
                  <img
                    src={getIconPath(item.id)}
                    onError={(e) =>
                      ((e.target as any).src = "/icons/placeholder.png")
                    }
                    className="w-5 h-5 object-contain"
                    alt=""
                  />
                  <span>{item.name}</span>
                </div>
              ))}
            </div>
          )}
        </div>

        <div>
          <label className="block text-xs font-bold uppercase tracking-wider text-neutral-400 mb-1.5">
            Target Yield (Units / Min)
          </label>
          <input
            type="number"
            value={props.amountInput}
            onChange={(e) => props.onAmountChange(e.target.value)}
            className="w-full bg-neutral-950 border border-neutral-700 rounded-lg px-3 py-2 text-white focus:outline-none focus:border-orange-500 transition-colors font-mono"
            min="0.01"
          />
        </div>

        <div className="flex flex-col flex-1">
          <label className="block text-xs font-bold uppercase tracking-wider text-neutral-400 mb-2">
            Unlocked Alternate Recipes
          </label>
          <div className="flex-1 min-h-[180px] bg-neutral-950 border border-neutral-800 rounded-lg p-3 overflow-y-auto flex flex-col gap-2">
            {props.alternates.length === 0 ? (
              <div className="text-xs text-neutral-600 italic p-2 text-center">
                Loading alternate database...
              </div>
            ) : (
              props.alternates.map((recipe) => (
                <label
                  key={recipe.id}
                  className={`flex items-center gap-3 p-2 rounded-md cursor-pointer border select-none transition-all text-xs ${
                    props.unlockedAlternates.includes(recipe.id)
                      ? "bg-orange-950/30 border-orange-500/40 text-orange-300"
                      : "bg-neutral-900/40 border-transparent hover:border-neutral-800 text-neutral-400"
                  }`}
                >
                  <input
                    type="checkbox"
                    checked={props.unlockedAlternates.includes(recipe.id)}
                    onChange={() => props.onToggleAlternate(recipe.id)}
                    className="accent-orange-500 w-3.5 h-3.5 rounded"
                  />
                  <div className="truncate pr-1">
                    <div className="font-semibold truncate">{recipe.name}</div>
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

      <div className="pt-4 border-t border-neutral-800 mt-4">
        <button
          onClick={props.onCalculate}
          disabled={props.loading}
          className="w-full bg-orange-600 hover:bg-orange-500 disabled:bg-neutral-800 disabled:text-neutral-600 text-neutral-950 font-black tracking-wide uppercase py-3 px-4 rounded-lg shadow-xl transition-colors cursor-pointer"
        >
          {props.loading ? "Analyzing Schematic..." : "Initialize Factory"}
        </button>
        {props.error && (
          <div className="mt-3 p-3 bg-red-950/40 border border-red-900 rounded-lg text-red-300 text-xs font-medium">
            {props.error}
          </div>
        )}
      </div>
    </div>
  );
}
