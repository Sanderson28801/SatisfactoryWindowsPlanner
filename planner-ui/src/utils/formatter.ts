export const getIconPath = (itemId: string | undefined): string => {
  // if (!itemId) return "/icons/placeholder.png";

  const formattedId = itemId.toLowerCase().replaceAll("_", "-");
  return `/icons/${formattedId}_64.png`;
};
