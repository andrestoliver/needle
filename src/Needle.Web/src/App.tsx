import { HomeFeaturesSection } from './components/HomeFeaturesSection';
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

            <HomeFeaturesSection />
        </main>
    );
}

export default App;


