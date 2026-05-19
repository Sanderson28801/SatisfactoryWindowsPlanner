export type NodeDto = {
  id: string;
  label: string;
  type: string;
  amount: number;
  machines: number;
  power: number;
};
export type EdgeDto = { source: string; target: string; amount: number };
export type GraphDto = { nodes: NodeDto[]; edges: EdgeDto[] };
export type MetaItem = { id: string; name: string };
