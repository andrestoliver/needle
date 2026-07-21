import { FeatureCard } from './components/FeatureCard';
import { homeFeatures } from './data/homeFeatures';
import './App.css';

function App() {
    return (
        <main className="app-shell">
            <section className="hero">
                <p className="eyebrow">Needle</p>
                <h1>Seu diário de álbuns</h1>
                <p className="hero-description">
                    Busque álbuns, importe favoritos para sua coleção e registre reviews
                    com notas de 0.5 a 5.0.
                </p>
            </section>

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
        </main>
    );
}

export default App;

