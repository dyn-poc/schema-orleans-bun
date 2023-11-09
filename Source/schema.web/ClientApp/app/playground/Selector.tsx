import { useState, MouseEvent } from 'react';
import samples, {type Sample, Samples} from '~/samples';

interface SelectorProps {
    onSelected: (data: any) => void;
}

export default function Selector({ onSelected }: SelectorProps) {
    const [current, setCurrent] = useState<Samples>('simple');

    function onLabelClick(label: Samples) {
        return (event: MouseEvent) => {
            event.preventDefault();
            setCurrent(label);
            setTimeout(() => onSelected(samples[label]), 0);
        };
    }

    return (
        <ul className='nav nav-pills'>
            {Object.keys(samples).map((label, i) => {
                return (
                    <li key={i} role='presentation' className={current === label ? 'active' : ''}>
                        <a href='#' onClick={onLabelClick(label as Samples)}>
                            {label}
                        </a>
                    </li>
                );
            })}
        </ul>
    );
}
