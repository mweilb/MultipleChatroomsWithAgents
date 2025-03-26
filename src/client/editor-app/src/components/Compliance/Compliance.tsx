import React from 'react';
import './Compliance.css';

const Compliance: React.FC = () => {
  return (
    <div className="compliance-container">
      <h1>Compliance and Ethical Oversight</h1>
      <p>
        In today's rapidly evolving technological landscape, it's not enough to simply create innovative products.
        We must also ensure that our solutions are safe, ethical, and ready for public use.
      </p>

      <section>
        <h2>Ethical Review</h2>
        <p>
          Ethical review is a critical step in our development process. Before deploying any new technology—especially those powered by artificial intelligence—
          we perform a thorough assessment to identify and mitigate potential biases and unintended consequences. This review helps ensure that:
        </p>
        <ul>
          <li>Data privacy and consent are rigorously maintained.</li>
          <li>Algorithmic decisions are transparent and explainable.</li>
          <li>Outcomes are fair and accountable to all stakeholders.</li>
          <li>Long-term societal impacts are carefully considered.</li>
        </ul>
      </section>

      <section>
        <h2>Security Notions in AI</h2>
        <p>
          As we integrate AI into our products, robust security measures become paramount. Our security strategy for AI includes:
        </p>
        <ul>
          <li>Protecting sensitive data from breaches and unauthorized access.</li>
          <li>Ensuring the integrity of our AI models against adversarial attacks.</li>
          <li>Continuous monitoring of systems to detect and respond to anomalies.</li>
          <li>Adhering to industry best practices and security frameworks.</li>
        </ul>
      </section>

      <section>
        <h2>Red Team Testing</h2>
        <p>
          To proactively secure our systems, we employ red team testing—a process where experts simulate real-world attacks on our products.
          This rigorous testing approach allows us to:
        </p>
        <ul>
          <li>Uncover vulnerabilities before they can be exploited.</li>
          <li>Validate the effectiveness of our security protocols.</li>
          <li>Gain insights into potential improvements for system defenses.</li>
          <li>Guarantee that outcomes remain favorable even under adversarial conditions.</li>
        </ul>
      </section>

      <section>
        <h2>Conclusion</h2>
        <p>
          It’s much easier to create something innovative than to ensure it is fully ready for public consumption.
          Through ethical reviews, robust AI security measures, and comprehensive red team testing, we commit to delivering
          products that are not only groundbreaking but also safe, secure, and ethically sound.
        </p>
      </section>
    </div>
  );
};

export default Compliance;
