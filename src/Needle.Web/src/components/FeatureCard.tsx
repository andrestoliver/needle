type FeatureCardProps = {
    number: string;
    title: string;
    description: string;
};

export function FeatureCard({
        number,
        title,
        description,
    }: FeatureCardProps) {
    return (
        <article className="card">
            <span>{number}</span>
            <h3>{title}</h3>
            <p>{description}</p>
        </article>
    );
}