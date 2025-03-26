import React, { useState } from 'react';
import { messageSectionDefinitions } from '../../../configs/SectionDefinitions';
import './Filters.css';

const Filters: React.FC = () => {
  const [hiddenSections, setHiddenSections] = useState<Set<string>>(new Set());
  const [hiddenFields, setHiddenFields] = useState<Set<string>>(new Set());

  // Toggle a specific section's visibility.
  const toggleSection = (key: string) => {
    setHiddenSections((prev) => {
      const updated = new Set(prev);
      if (updated.has(key)) {
        updated.delete(key);
        window.dispatchEvent(
          new CustomEvent('toggleSectionVisibility', {
            detail: { sectionTitle: key, visible: true },
          })
        );
      } else {
        updated.add(key);
        window.dispatchEvent(
          new CustomEvent('toggleSectionVisibility', {
            detail: { sectionTitle: key, visible: false },
          })
        );
      }
      return updated;
    });
  };

  // Toggle a field label's visibility.
  const toggleField = (label: string) => {
    setHiddenFields((prev) => {
      const updated = new Set(prev);
      if (updated.has(label)) {
        updated.delete(label);
        window.dispatchEvent(
          new CustomEvent('toggleFieldVisibility', {
            detail: { fieldLabel: label, visible: true },
          })
        );
      } else {
        updated.add(label);
        window.dispatchEvent(
          new CustomEvent('toggleFieldVisibility', {
            detail: { fieldLabel: label, visible: false },
          })
        );
      }
      return updated;
    });
  };

  // Global toggles.
  const showAll = () => {
    setHiddenSections(new Set());
    setHiddenFields(new Set());
    messageSectionDefinitions.forEach((section) => {
      window.dispatchEvent(
        new CustomEvent('toggleSectionVisibility', {
          detail: { sectionTitle: section.key, visible: true },
        })
      );
      section.groups?.forEach((group) => {
        window.dispatchEvent(
          new CustomEvent('toggleSectionVisibility', {
            detail: { sectionTitle: group.key, visible: true },
          })
        );
      });
    });
    // Show all fields.
    const allFields = new Set<string>();
    messageSectionDefinitions.forEach((section) => {
      section.fields?.forEach((field) => allFields.add(field.label));
      section.groups?.forEach((group) =>
        group.fields.forEach((field) => allFields.add(field.label))
      );
    });
    allFields.forEach((label) => {
      window.dispatchEvent(
        new CustomEvent('toggleFieldVisibility', {
          detail: { fieldLabel: label, visible: true },
        })
      );
    });
  };

  const hideAll = () => {
    const allSectionKeys = messageSectionDefinitions.reduce<string[]>((acc, section) => {
      acc.push(section.key);
      section.groups?.forEach((group) => acc.push(group.key));
      return acc;
    }, []);
    setHiddenSections(new Set(allSectionKeys));
    messageSectionDefinitions.forEach((section) => {
      window.dispatchEvent(
        new CustomEvent('toggleSectionVisibility', {
          detail: { sectionTitle: section.key, visible: false },
        })
      );
      section.groups?.forEach((group) => {
        window.dispatchEvent(
          new CustomEvent('toggleSectionVisibility', {
            detail: { sectionTitle: group.key, visible: false },
          })
        );
      });
    });
    // Hide all fields.
    const allFields = new Set<string>();
    messageSectionDefinitions.forEach((section) => {
      section.fields?.forEach((field) => allFields.add(field.label));
      section.groups?.forEach((group) =>
        group.fields.forEach((field) => allFields.add(field.label))
      );
    });
    setHiddenFields(allFields);
    allFields.forEach((label) => {
      window.dispatchEvent(
        new CustomEvent('toggleFieldVisibility', {
          detail: { fieldLabel: label, visible: false },
        })
      );
    });
  };

  // Compute unique field labels.
  const uniqueFieldLabels = Array.from(
    messageSectionDefinitions.reduce((acc, section) => {
      section.fields?.forEach((field) => acc.add(field.label));
      section.groups?.forEach((group) =>
        group.fields.forEach((field) => acc.add(field.label))
      );
      return acc;
    }, new Set<string>())
  );

  return (
    <div className="filters-container compact">
      <div className="filter-header">
        <h3>Filter Settings</h3>
        <div className="global-buttons">
          <button onClick={showAll} className="btn">Show All</button>
          <button onClick={hideAll} className="btn">Hide All</button>
        </div>
      </div>
      <div className="filter-sections">
        <h4>Sections</h4>
        <div className="sections-grid">
          {messageSectionDefinitions.map((section) => (
            <div key={section.key} className="section-item">
              <button onClick={() => toggleSection(section.key)} className="btn">
                {hiddenSections.has(section.key) ? `Show ${section.title}` : `Hide ${section.title}`}
              </button>
              {section.groups && section.groups.length > 0 && (
                <div className="subsections">
                  {section.groups.map((group) => (
                    <button
                      key={group.key}
                      onClick={() => toggleSection(group.key)}
                      className="btn sub-btn"
                    >
                      {hiddenSections.has(group.key) ? `Show ${group.name}` : `Hide ${group.name}`}
                    </button>
                  ))}
                </div>
              )}
            </div>
          ))}
        </div>
      </div>
      <div className="filter-fields">
        <h4>Fields</h4>
        <div className="fields-grid">
          {uniqueFieldLabels.map((label) => (
            <button key={label} onClick={() => toggleField(label)} className="btn">
              {hiddenFields.has(label) ? `Show ${label}` : `Hide ${label}`}
            </button>
          ))}
        </div>
      </div>
    </div>
  );
};

export default Filters;
