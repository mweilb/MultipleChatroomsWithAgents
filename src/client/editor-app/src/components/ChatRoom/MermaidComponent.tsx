import mermaid from "mermaid";
import React from "react";
import svgPanZoom from "svg-pan-zoom";

mermaid.initialize({
  startOnLoad: true,
  theme: "default",
  securityLevel: "loose",
  fontFamily: "monospace",
});

interface MermaidComponentProps {
  chart: string;
}

export default class MermaidComponent extends React.Component<MermaidComponentProps> {
  containerRef = React.createRef<HTMLDivElement>();
  panZoomInstance: any = null;

  async renderMermaid() {
    if (!this.containerRef.current) return;
    // Create a unique id for each render
    const uniqueId = `mermaid-${Date.now()}`;
    try {
      // Use mermaid.render which now returns a Promise
      const renderResult = await mermaid.render(uniqueId, this.props.chart);
      if (this.containerRef.current) {
        this.containerRef.current.innerHTML = renderResult.svg;
        // Look for the rendered SVG element in the container
        const svgElement = this.containerRef.current.querySelector("svg");
        if (svgElement) {
          // Destroy any existing pan/zoom instance before creating a new one
          if (this.panZoomInstance) {
            this.panZoomInstance.destroy();
          }
          // Initialize pan and zoom on the SVG element
          this.panZoomInstance = svgPanZoom(svgElement, {
            zoomEnabled: true,
            controlIconsEnabled: true,
            fit: true,
            center: true,
          });
        }
      }
    } catch (error) {
      console.error("Error rendering Mermaid diagram:", error);
    }
  }

  componentDidMount() {
    this.renderMermaid();
  }

  componentDidUpdate(prevProps: MermaidComponentProps) {
    if (prevProps.chart !== this.props.chart) {
      this.renderMermaid();
    }
  }

  componentWillUnmount() {
    if (this.panZoomInstance) {
      this.panZoomInstance.destroy();
    }
  }

  render() {
    return <div ref={this.containerRef} className="mermaid" />;
  }
}
