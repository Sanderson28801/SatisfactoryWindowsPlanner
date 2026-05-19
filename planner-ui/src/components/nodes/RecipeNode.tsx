import React from "react";
import { Handle, Position } from "reactflow";
import { getIconPath } from "../../utils/formatter";

export const RecipeNode = ({ data }: { data: any }) => {
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
