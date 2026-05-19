import { type MetaItem } from "../../types";

type Props = {
  alternates: MetaItem[];
  unlocked: string[];
  onToggle: (id: string) => void;
};

export function AlternateList({ alternates, unlocked, onToggle }: Props) {
  return (
    <div className="flex flex-col flex-1">
      <label className="block text-xs font-bold uppercase tracking-wider text-neutral-400 mb-2">
        Unlocked Alternate Recipes
      </label>
      <div className="flex-1 min-h-[180px] bg-neutral-950 border border-neutral-800 rounded-lg p-3 overflow-y-auto flex flex-col gap-2">
        {alternates.length === 0 ? (
          <div className="text-xs text-neutral-600 italic p-2 text-center">
            Loading database...
          </div>
        ) : (
          alternates.map((recipe) => (
            <label
              key={recipe.id}
              className={`flex items-center gap-3 p-2 rounded-md cursor-pointer border select-none transition-all text-xs ${
                unlocked.includes(recipe.id)
                  ? "bg-orange-950/30 border-orange-500/40 text-orange-300"
                  : "bg-neutral-900/40 border-transparent hover:border-neutral-800 text-neutral-400"
              }`}
            >
              <input
                type="checkbox"
                checked={unlocked.includes(recipe.id)}
                onChange={() => onToggle(recipe.id)}
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
  );
}
