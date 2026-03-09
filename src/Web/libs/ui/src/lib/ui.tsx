import styles from './ui.module.css';

/**
 * Renders the Ui component showing a container with a welcoming heading.
 *
 * @returns The React element tree for the Ui component.
 */
export function Ui() {
  return (
    <div className={styles['container']}>
      <h1>Welcome to Ui!</h1>
    </div>
  );
}

export default Ui;
