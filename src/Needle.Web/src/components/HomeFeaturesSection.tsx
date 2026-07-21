import { homeFeatures } from '../data/homeFeatures';
import { FeatureCard } from './FeatureCard';

export function HomeFeaturesSection() {
    return (
        <section className="next-steps" aria-labelledby="next-steps-title">
            <h2 id="next-steps-title">Primeiros fluxos</h2>

            <div className="cards">
                {homeFeatures.map((feature) => (
                    <FeatureCard
                        key={feature.number}
                        number={feature.number}
                        title={feature.title}
                        description={feature.description}
                    />
                ))}
            </div>
        </section>
    );
}