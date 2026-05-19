import { useState } from "react";
import { type MetaItem } from "../../types";
import { getIconPath } from "../../utils/formatter";

type Props = {
  items: MetaItem[];
  selectedItemId: string;
  onSelectItem: (id: string) => void;
};

export function ItemSelect({ items, selectedItemId, onSelectItem }: Props) {
  const [searchQuery, setSearchQuery] = useState("");
  const [isOpen, setIsOpen] = useState(false);

  const filteredItems = items.filter((item) =>
    item.name.toLowerCase().includes(searchQuery.toLowerCase())
  );

  return (
    <div className="relative">
      <label className="block text-xs font-bold uppercase tracking-wider text-neutral-400 mb-1.5">
        Target Output Product
      </label>
      <div
        onClick={() => setIsOpen(!isOpen)}
        className="w-full bg-neutral-950 border border-neutral-700 rounded-lg px-3 py-2.5 text-left flex items-center justify-between cursor-pointer hover:border-neutral-500 transition-colors"
      >
        <div className="flex items-center gap-2 truncate">
          {selectedItemId && (
            <img
              src={getIconPath(selectedItemId)}
              className="w-5 h-5 object-contain"
              alt=""
            />
          )}
          <span>
            {selectedItemId
              ? items.find((i) => i.id === selectedItemId)?.name
              : "Choose an item..."}
          </span>
        </div>
        <span className="text-neutral-500 text-xs">▼</span>
      </div>

      {isOpen && (
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
                onSelectItem(item.id);
                setIsOpen(false);
                setSearchQuery("");
              }}
              className="flex items-center gap-2 px-2 py-1.5 rounded hover:bg-orange-500 cursor-pointer text-sm"
            >
              <img
                src={getIconPath(item.id)}
                className="w-5 h-5 object-contain"
                alt=""
              />
              <span>{item.name}</span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
