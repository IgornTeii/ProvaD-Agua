import type { ReactNode } from "react";
import { Info } from "lucide-react";

interface PageHeaderProps {
  title: string;
  description: ReactNode;
  tip?: ReactNode;
}

export default function PageHeader({ title, description, tip }: PageHeaderProps) {
  return (
    <header className="page-header">
      <h1>{title}</h1>
      <p className="lead">{description}</p>
      {tip && (
        <p className="tip">
          <Info size={20} aria-hidden="true" />
          <span>{tip}</span>
        </p>
      )}
    </header>
  );
}
