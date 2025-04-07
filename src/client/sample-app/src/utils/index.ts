export const getColorByMessage = (() => {
  const colorMap = new Map<string, string>();

  return (transactionId: string, color: string): string => {
    if (!colorMap.has(transactionId)) {
      colorMap.set(transactionId, color);
    }
    return colorMap.get(transactionId)!;
  };
})();